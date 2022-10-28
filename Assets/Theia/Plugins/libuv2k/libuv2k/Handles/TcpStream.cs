using AOT;
using System;
using System.Net;
using System.Runtime.InteropServices;
using libuv2k.Native;

namespace libuv2k
{
    public sealed class TcpStream : NativeHandle
    {
        internal const int DefaultBacklog = 128;

        // IMPORTANT: converting our C# functions to uv_alloc_cb etc. allocates!
        // let's only do this once here and then use the converted ones everywhere.
        internal static readonly uv_alloc_cb AllocateCallback = OnAllocateCallback;
        internal static readonly uv_read_cb ReadCallback = OnReadCallback;
        internal static readonly uv_watcher_cb ConnectionCallback = OnConnectionCallback;
        internal static readonly uv_watcher_cb WriteCallback = OnWriteCallback;
        Action<ArraySegment<byte>> FramingCallback;

        // the loop which this stream runs in
        Loop loop;

        // handle
        public bool IsActive => IsValid && NativeMethods.IsHandleActive(Handle);
        public bool IsClosing => IsValid && NativeMethods.IsHandleClosing(Handle);

        // callbacks
        public Action<TcpStream, Exception> onClientConnect;
        public Action<TcpStream, Exception> onServerConnect;
        // onMessage passes an ArraySegment to avoid byte copying.
        // the segment is valid only until onData returns.
        // we use framing, so the segment is the actual message, not just random
        // stream data.
        public Action<TcpStream, ArraySegment<byte>> onMessage;
        public Action<TcpStream, Exception> onError;
        // onClosed is called after we closed the stream
        public Action<TcpStream> onClosed;

        // pinned readBuffer for pending reads
        // libuv Allocate+Read callbacks always go in pairs:
        //   https://github.com/libuv/libuv/issues/1085
        //   https://libuv.narkive.com/cn1gwvvA/passing-a-small-buffer-to-alloc-cb-results-in-multiple-calls-to-alloc-cb
        // So we allocate the buffer only once, and then reuse it.
        // we also pin it so that GC doesn't move it while libuv is reading.
        byte[] readBuffer;
        internal GCHandle readBufferPin;
        uv_buf_t readBufferStruct; // for libuv native C

        // user data
        public object UserToken { get; set; }

        // framing buffer.
        // read buffer is of ReceiveBufferSize (~408KB on mac).
        // framing buffer is of MaxMessageWithHeaderSize to build messages into.
        byte[] framingBuffer = new byte[MaxMessageWithHeaderSize];
        int framingBufferPosition = 0;

        // payload buffer for message sending.
        // we need a buffer to construct <length:4, data:length> packets.
        byte[] payloadBuffer = new byte[MaxMessageWithHeaderSize];

        // libuv uses good internal send/recv buffer sizes which we should not
        // change. we use the internal recv buffer size for our receive buffer.
        // for WriteRequest we need MaxMessageSize. using SendBufferSize would
        // not make sense since WriteRequests always write up to MaxMessageSize.
        // this way we don't need to worry about runtime send buffer size
        // changes either (Linux does a lot of buffer size magic).
        //
        // we will also need MaxMessageSize for framing.
        // 64KB as ushort.max for optimal Mirror support!
        public const int MaxMessageSize = ushort.MaxValue;
        public const int MaxMessageWithHeaderSize = MaxMessageSize + 4;

        // we need a watcher request for ongoing connects to get notified about
        // the result
        WatcherRequest connectRequest;

        // one static WriteRequest pool to avoid allocations
        // note: has to be static because OnWriteCallback needs to be static.
        internal static readonly Pool<WriteRequest> WriteRequestPool =
            new Pool<WriteRequest>(
                () => new WriteRequest(),
                (obj) => obj.Dispose()
            );

        // constructor /////////////////////////////////////////////////////////
        public TcpStream(Loop loop)
            // allocate base NativeHandle with size of UV_TCP
            : base(NativeMethods.GetSize(uv_handle_type.UV_TCP))
        {
            // make sure loop can be used
            loop.Validate();
            this.loop = loop;

            // converting OnFramingCallback to Action allocates.
            // lets do it only once, instead of every ReadCallback.
            FramingCallback = OnFramingCallback;

            if (loop.Handle != IntPtr.Zero)
            {
                try
                {
                    int result = NativeMethods.InitializeTcp(loop.Handle, Handle);
                    NativeMethods.ThrowIfError(result);
                }
                catch (Exception)
                {
                    FreeHandle(ref Handle);
                    throw;
                }

                // note: setting send/recv buffer sizes gives EBADF when trying it
                // here. instead we call ConfigureSendReceiveBufferSize after
                // successful connects.
            }
            else throw new ArgumentException($"{nameof(TcpStream)} loop.Handle can't be zero!");
        }

        // creates a new TcpStream for a connecting client
        internal TcpStream NewStream()
        {
            TcpStream client = new TcpStream(loop);
            NativeMethods.StreamAccept(Handle, client.Handle);
            client.ReadStart();
            //Logger.Log($"TcpStream {InternalHandle} client {client.InternalHandle} accepted.");
            return client;
        }

        // pinned read buffer //////////////////////////////////////////////////
        internal uv_buf_t PinReadBuffer()
        {
            // pin it so GC doesn't dispose or move it while libuv is using
            // it.
            //
            // the assert is extremely slow, and it allocates.
            //Debug.Assert(!this.pin.IsAllocated);
            readBufferPin = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
            IntPtr arrayHandle = readBufferPin.AddrOfPinnedObject();
            return new uv_buf_t(arrayHandle, readBuffer.Length);
        }

        internal void UnpinReadBuffer()
        {
            if (readBufferPin.IsAllocated)
            {
                readBufferPin.Free();
            }
        }

        // tcp configuration ///////////////////////////////////////////////////
        public void NoDelay(bool value)
        {
            Validate();
            NativeMethods.TcpSetNoDelay(Handle, value);
        }

        public void KeepAlive(bool value, int delay)
        {
            Validate();
            NativeMethods.TcpSetKeepAlive(Handle, value, delay);
        }

        public void SimultaneousAccepts(bool value)
        {
            Validate();
            NativeMethods.TcpSimultaneousAccepts(Handle, value);
        }

        // find out libuv's send buffer size
        public int GetSendBufferSize()
        {
            Validate();
            return NativeMethods.SendBufferSize(Handle, 0);
        }

        // find out libuv's recv buffer size
        public int GetReceiveBufferSize()
        {
            Validate();
            return NativeMethods.ReceiveBufferSize(Handle, 0);
        }

        // buffers /////////////////////////////////////////////////////////////
        // create read buffer and pin it so GC doesn't move it while libuv uses
        // it.
        internal void CreateAndPinReadBuffer(int size)
        {
            readBuffer = new byte[size];
            readBufferStruct = PinReadBuffer();
        }

        // libuv uses 408KB recv buffer and 146KB send buffer on OSX by default.
        // that's plenty of space for batching, and there is no easy way to find
        // good send/recv buffer values across all platforms (especially linux).
        // let libuv handle it, don't allow modifying them.
        // (which makes allocation free byte[] pooling easier too)
        //
        //   internal int SetSendBufferSize(int value)
        //   {
        //       Validate();
        //       return NativeMethods.SendBufferSize(InternalHandle, value);
        //   }
        //   internal int SetReceiveBufferSize(int value)
        //   {
        //       Validate();
        //       return NativeMethods.ReceiveBufferSize(InternalHandle, value);
        //   }

        // read buffer should be initialized to libuv's buffer sizes as soon as
        // we can access them safely without EBADF after connecting.
        // we also log both sizes so the user knows what to expect.
        internal void InitializeInternalBuffers()
        {
            int sendSize = GetSendBufferSize();
            int recvSize = GetReceiveBufferSize();
            Log.Info($"libuv send buffer size = {sendSize}, recv buffer size = {recvSize}");

            // create read buffer and pin it so GC doesn't move it while libuv
            // uses it.
            if (readBuffer == null)
            {
                CreateAndPinReadBuffer(recvSize);
            }
            else Log.Error($"{nameof(InitializeInternalBuffers)} called twice. That should never happen.");
        }

        // endpoints ///////////////////////////////////////////////////////////
        public IPEndPoint GetLocalEndPoint()
        {
            Validate();
            return NativeMethods.TcpGetSocketName(Handle);
        }

        public IPEndPoint GetPeerEndPoint()
        {
            Validate();
            return NativeMethods.TcpGetPeerName(Handle);
        }

        // bind ////////////////////////////////////////////////////////////////
        public void Bind(IPEndPoint endPoint, bool dualStack = false)
        {
            if (endPoint != null)
            {
                Validate();
                NativeMethods.TcpBind(Handle, endPoint, dualStack);
            }
            else throw new ArgumentException("EndPoint can't be null!");
        }

        // listen //////////////////////////////////////////////////////////////
        public void Listen(int backlog = DefaultBacklog)
        {
            // make sure callback is setup. won't get notified otherwise.
            if (onServerConnect != null && backlog > 0)
            {
                Validate();
                try
                {
                    NativeMethods.StreamListen(Handle, backlog);
                    //Log.Info($"Stream TcpStream {InternalHandle} listening, backlog = {backlog}");
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }
            else throw new ArgumentException($"onServerConnect, backlog can't be null!");
        }

        public void Listen(IPEndPoint localEndPoint, int backlog = DefaultBacklog, bool dualStack = false)
        {
            // make sure callback is setup. won't get notified otherwise.
            if (localEndPoint != null && onServerConnect != null)
            {
                Bind(localEndPoint, dualStack);
                Listen(backlog);
            }
            else throw new ArgumentException($"onServerConnect, localEndPoint can't be null!");
        }

        // connect /////////////////////////////////////////////////////////////
        void ConnectRequestCallback(WatcherRequest request, Exception error)
        {
            try
            {
                if (error == null)
                {
                    // initialize internal buffer after we can access libuv
                    // send/recv buffer sizes (which is after connecting)
                    InitializeInternalBuffers();
                    ReadStart();
                }

                onClientConnect(this, error);
            }
            finally
            {
                connectRequest.Dispose();
                connectRequest = null;
            }
        }

        public void ConnectTo(IPEndPoint remoteEndPoint)
        {
            // make sure callback has been set up. won't get notified otherwise.
            if (onClientConnect != null && remoteEndPoint != null)
            {
                // let's only connect if a connect isn't in progress already.
                // this way we only need to keep track of one WatcherRequest.
                if (connectRequest == null)
                {
                    try
                    {
                        connectRequest = new WatcherRequest(
                            uv_req_type.UV_CONNECT,
                            ConnectRequestCallback,
                            h => NativeMethods.TcpConnect(h, Handle, remoteEndPoint));
                    }
                    catch (Exception)
                    {
                        connectRequest?.Dispose();
                        connectRequest = null;
                        throw;
                    }
                }
                else Log.Warning("A connect is already in progress. Please wait for it to finish before connecting again.");
            }
            else throw new ArgumentException($"onClientConnect, remoteEndPoint can't be null!");
        }

        public void ConnectTo(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, bool dualStack = false)
        {
            if (localEndPoint != null && remoteEndPoint != null)
            {
                Bind(localEndPoint, dualStack);
                ConnectTo(remoteEndPoint);
            }
            else throw new ArgumentException($"localEndPoint, remoteEndPoint can't be null!");
        }

        // write ///////////////////////////////////////////////////////////////
        // WriteCallback is called after writing.
        // we use it to return the WriteRequest to the pool in order to avoid
        // allocations.
        //
        // C->C# callbacks need to be static and have MonoPInvokeCallback for
        // IL2CPP builds. avoids "System.NotSupportedException: To marshal a
        // managed method please add an attribute named 'MonoPInvokeCallback'"
        [MonoPInvokeCallback(typeof(uv_watcher_cb))]
        static void OnWriteCallback(IntPtr handle, int status)
        {
            // get from native lookup
            if (nativeLookup.TryGetValue(handle, out NativeHandle entry))
            {
                if (entry is WriteRequest writeRequest)
                {
                    WriteRequestPool.Return(writeRequest);
                }
                else Log.Error($"TcpStream.OnWriteCallback: unexpected lookup type: {entry.GetType()}");
            }
            else Log.Error($"TcpStream.OnWriteCallback: no nativeLookup entry found for handle={handle}!");
        }

        // WriteStream writes the final WriteRequest
        internal unsafe void WriteStream(WriteRequest request)
        {
            if (request != null)
            {
                Validate();
                try
                {
                    // WriteRequest could keep multiple buffers, but only keeps one
                    // so we always pass '1' as size
                    NativeMethods.WriteStream(request.Handle, Handle, request.Bufs, 1, WriteCallback);
                }
                catch (Exception exception)
                {
                    Log.Error($"TcpStream Failed to write data {request}: {exception}");
                    throw;
                }
            }
            else throw new ArgumentException("Request can't be null!");
        }

        // segment data is copied internally and can be reused immediately.
        public void Send(ArraySegment<byte> segment)
        {
            // make sure we don't try to write anything larger than WriteRequest
            // internal buffer size, which is MaxMessageSize + 4 for header
            if (segment.Count <= MaxMessageSize)
            {
                // create <size, data> payload so we only call write once.
                if (Framing.Frame(payloadBuffer, segment))
                {
                    // queue write the payload
                    ArraySegment<byte> payload = new ArraySegment<byte>(payloadBuffer, 0, segment.Count + 4);
                    WriteRequest request = WriteRequestPool.Take();
                    try
                    {
                        // prepare request with our completion callback, and make
                        // sure that streamHandle is passed as first parameter.
                        request.Prepare(payload);
                        WriteStream(request);
                    }
                    catch (Exception exception)
                    {
                        Log.Error($"TcpStream faulted: {exception}");
                        WriteRequestPool.Return(request);
                        throw;
                    }
                }
                else Log.Error($"Framing failed for message with size={segment.Count}. Make sure it's within MaxMessageSize={MaxMessageSize}");
            }
            else Log.Error($"Failed to send message of size {segment.Count} because it's larger than MaxMessageSize={MaxMessageSize}");
        }

        // read ////////////////////////////////////////////////////////////////
        internal void ReadStart()
        {
            Validate();
            NativeMethods.StreamReadStart(Handle);
        }

        internal void ReadStop()
        {
            if (!IsValid)
            {
                return;
            }

            // This function is idempotent and may be safely called on a stopped stream.
            NativeMethods.StreamReadStop(Handle);
        }

        // callbacks ///////////////////////////////////////////////////////////
        // C->C# callbacks need to be static and have MonoPInvokeCallback for
        // IL2CPP builds. avoids "System.NotSupportedException: To marshal a
        // managed method please add an attribute named 'MonoPInvokeCallback'"
        [MonoPInvokeCallback(typeof(uv_watcher_cb))]
        static void OnConnectionCallback(IntPtr handle, int status)
        {
            // look up the C# TcpStream for this native handle
            if (nativeLookup.TryGetValue(handle, out NativeHandle entry))
            {
                if (entry is TcpStream server)
                {
                    TcpStream client = null;
                    Exception error = null;
                    try
                    {
                        if (status < 0)
                        {
                            error = NativeMethods.CreateError((uv_err_code)status);
                        }
                        else
                        {
                            client = server.NewStream();

                            // initialize internal buffer after we can access libuv
                            // send/recv buffer sizes (which is after connecting)
                            client.InitializeInternalBuffers();
                        }

                        server.onServerConnect(client, error);
                    }
                    catch
                    {
                        client?.Dispose();
                        throw;
                    }
                }
                else Log.Error($"TcpStream.OnConnectionCallback: unexpected lookup type: {entry.GetType()}");
            }
            else Log.Error($"TcpStream.OnConnectionCallback: nativeLookup no entry found for handle={handle}");
        }

        // simply callback for message framing that calls onMessage
        void OnFramingCallback(ArraySegment<byte> segment)
        {
            onMessage(this, segment);
        }

        void OnReadCallback(byte[] buffer, int status)
        {
            //Log.Info($"OnReadCallback TcpStream {InternalHandle} status={status} buffer={byteBuffer.ReadableBytes}");

            // status >= 0 means there is data available
            if (status >= 0)
            {
                //Log.Info($"TcpStream {InternalHandle} read, buffer length = {byteBuffer.Capacity} status = {status}.");

                // bytes to read within buffer length?
                if (status <= buffer.Length)
                {
                    // framing: we will receive bytes from a stream and we need
                    // to cut it back into messages manually based on header.
                    if (!Framing.Unframe(framingBuffer, ref framingBufferPosition, new ArraySegment<byte>(buffer, 0, status), MaxMessageSize, FramingCallback))
                    {
                        // framing failed because of invalid data / exploits / attacks
                        // let's disconnect
                        Log.Warning($"Unframe failed because of invalid data, potentially because of a header attack. Disconnecting {Handle}");

                        // TODO reuse with below code

                        onError(this, new Exception("Unframe failed"));

                        // stop reading either way
                        ReadStop();

                        // close the connection
                        // (Dispose calls CloseHandle)
                        Dispose();
                    }
                }
                else Log.Error($"OnReadCallback failed because buffer with length {buffer.Length} is too small for status={status}");
            }
            // status < 0 means error
            else
            {
                // did we encounter a serious error?
                // * UV_EOF means stream was closed, in which case we should
                //   call onCompleted but we don't need to call on onError
                //   because closing a stream is normal behaviour
                // * ECONNRESET happens if the other end of the network
                //   suddenly killed the client.
                if (status != (int)uv_err_code.UV_EOF &&
                    status != (int)uv_err_code.UV_ECONNRESET)
                {
                    Exception exception = NativeMethods.CreateError((uv_err_code)status);
                    Log.Error($"TcpStream {Handle} read error: {status}: {exception}");
                    onError(this, exception);
                }

                //Log.Info($"TcpStream {Handle} stream completed");

                // stop reading either way
                ReadStop();

                // close the connection
                // (Dispose calls CloseHandle)
                Dispose();
            }
        }

        // C->C# callbacks need to be static and have MonoPInvokeCallback for
        // IL2CPP builds. avoids "System.NotSupportedException: To marshal a
        // managed method please add an attribute named 'MonoPInvokeCallback'"
        [MonoPInvokeCallback(typeof(uv_read_cb))]
        static void OnReadCallback(IntPtr handle, IntPtr nread, ref uv_buf_t buf)
        {
            // look up the C# TcpStream for this native handle
            if (nativeLookup.TryGetValue(handle, out NativeHandle entry))
            {
                if (entry is TcpStream stream)
                {
                    stream.OnReadCallback(stream.readBuffer, (int)nread.ToInt64());
                }
                else Log.Error($"TcpStream.OnReadCallback: unexpected lookup type: {entry.GetType()}");
            }
            else Log.Error($"TcpStream.OnReadCallback: nativeLookup no entry found for handle={handle}");
        }

        // C->C# callbacks need to be static and have MonoPInvokeCallback for
        // IL2CPP builds. avoids "System.NotSupportedException: To marshal a
        // managed method please add an attribute named 'MonoPInvokeCallback'"
        [MonoPInvokeCallback(typeof(uv_alloc_cb))]
        static void OnAllocateCallback(IntPtr handle, IntPtr suggestedSize, out uv_buf_t buf)
        {
            // look up the C# TcpStream for this native handle
            buf = default;
            if (nativeLookup.TryGetValue(handle, out NativeHandle entry))
            {
                if (entry is TcpStream stream)
                {
                    buf = stream.readBufferStruct;
                }
                else Log.Error($"TcpStream.OnReadCallback: unexpected lookup type: {entry.GetType()}");
            }
            else Log.Error($"TcpStream.OnReadCallback: nativeLookup no entry found for handle={handle}");
        }

        // cleanup /////////////////////////////////////////////////////////////
        // callback for uv_close
        static readonly uv_close_cb CloseCallback = OnCloseHandle;

        // CloseHandle called by NativeHandle, calls uv_close with callback
        protected override void CloseHandle()
        {
            // dispose connect request (if any)
            connectRequest?.Dispose();

            // uv_tcp_init in constructor requires us to call uv_close here
            // before freeing base NativeHandle
            //
            // IMPORTANT: CloseCallback is not called IMMEDIATELY!
            // it happens some time after returning.
            NativeMethods.CloseHandle(Handle, CloseCallback);
            //Logger.Log($"TcpStream {Handle} closed, releasing pending resources.");
        }

        // C->C# callbacks need to be static and have MonoPInvokeCallback for
        // IL2CPP builds. avoids "System.NotSupportedException: To marshal a
        // managed method please add an attribute named 'MonoPInvokeCallback'"
        [MonoPInvokeCallback(typeof(uv_close_cb))]
        static void OnCloseHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                return;

            // remove from native lookup and call OnHandleClosed
            if (nativeLookup.TryGetValue(handle, out NativeHandle entry))
            {
                if (entry is TcpStream stream)
                {
                    // unpin read buffer now that uv_close finished and we
                    // can be sure that it's not used anymore
                    stream.UnpinReadBuffer();

                    try
                    {
                        // call onClosed event now that uv_close callback finished
                        stream.onClosed?.Invoke(stream);
                    }
                    catch (Exception exception)
                    {
                        Log.Error($"TcpStream.OnCloseHandle error: {exception}");
                    }
                }
                else Log.Error($"TcpStream.OnCloseHandle: unexpected lookup type: {entry.GetType()}");
            }
            else Log.Error($"TcpStream.OnCloseHandle: no nativeLookup entry found for handle={handle}!");

            // free base NativeHandle
            FreeHandle(ref handle);
        }
    }
}
