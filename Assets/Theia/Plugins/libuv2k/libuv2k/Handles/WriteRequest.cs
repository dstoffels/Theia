using System;
using System.Runtime.InteropServices;
using libuv2k.Native;

namespace libuv2k
{
    sealed unsafe class WriteRequest : NativeHandle
    {
        static readonly int HandleSize = NativeMethods.GetSize(uv_req_type.UV_WRITE);
        static readonly int BufferSize = Marshal.SizeOf<uv_buf_t>();

        internal GCHandle dataPin;
        internal IntPtr dataPinAddress;

        internal GCHandle pin;

        internal uv_buf_t* Bufs;

        // Prepare() gets an ArraySegment, but we can't assume that it persists
        // until the completed callback. so we need to copy into an internal
        // buffer.
        // we use MaxMessageSize + 4 bytes header for every WriteRequest.
        // this way we avoid allocations since WriteRequest itself is pooled!
        internal byte[] data = new byte[TcpStream.MaxMessageWithHeaderSize];

        // UV_WRITE is for TCP write. UDP would require a different request type.
        internal WriteRequest()
            // allocate base NativeHandle with size of UV_WRITE + BufferSize
            : base(HandleSize + BufferSize)
        {
            if (BufferSize >= 0)
            {
                // pin request handle
                // Note: WriteRequest is pooled. pinning doesn't happen in HOT PATH!
                // TODO why does this need to be pinned? (maybe because uv_buf_t*?)
                IntPtr HandleAddress = Handle;
                Bufs = (uv_buf_t*)(HandleAddress + HandleSize);
                pin = GCHandle.Alloc(HandleAddress, GCHandleType.Pinned);

                // pin the data array once and keep it pinned until the end.
                // no need to unpin before returning to pool and pin again in
                // Prepare like we did originally.
                // => data is always same size
                // => pinning only once is faster
                dataPin = GCHandle.Alloc(data, GCHandleType.Pinned);
                dataPinAddress = dataPin.AddrOfPinnedObject();
            }
            else throw new ArgumentException($"BufferSize {BufferSize} needs to be >=0");
        }

        internal void Prepare(ArraySegment<byte> segment)
        {
            if (!IsValid)
            {
                throw new ObjectDisposedException("WriteRequest status is invalid.");
            }

            // we can't assume that the ArraySegment that we passed in Send()
            // will persist until OnCompleted (in Mirror/DOTSNET it won't).
            // so we need to copy it to our internal data buffer.
            if (segment.Count > data.Length)
            {
                throw new ArgumentException("Segment.Count=" + segment.Count + " is too big for fixed internal buffer size=" + data.Length);
            }
            Buffer.BlockCopy(segment.Array, segment.Offset, data, 0, segment.Count);

            // 'data' was pinned in constructor already, no need to pin it again
            // here because the capacity did not change. simply init the libuv
            // memory.
            uv_buf_t.InitMemory((IntPtr)Bufs, dataPinAddress, segment.Count);
        }

        internal void Free()
        {
            // release pinned request handle
            if (pin.IsAllocated)
            {
                pin.Free();
            }

            // release pinned data array
            if (dataPin.IsAllocated)
            {
                dataPin.Free();
                dataPinAddress = IntPtr.Zero;
            }

            Bufs = (uv_buf_t*)IntPtr.Zero;
        }

        protected override void CloseHandle()
        {
            if ((IntPtr)Bufs != IntPtr.Zero)
            {
                Free();
            }

            // free base NativeHandle
            FreeHandle(ref Handle);
        }
    }
}
