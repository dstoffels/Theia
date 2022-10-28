using AOT;
using System;
using libuv2k.Native;

namespace libuv2k
{
    public sealed class WatcherRequest : NativeHandle
    {
        internal static readonly uv_watcher_cb WatcherCallback = OnWatcherCallback;
        readonly bool closeOnCallback;
        Action<WatcherRequest, Exception> watcherCallback;

        uv_req_type RequestType;

        internal WatcherRequest(
            uv_req_type requestType,
            Action<WatcherRequest, Exception> watcherCallback,
            Action<IntPtr> initializer,
            bool closeOnCallback = false)
            // allocate base NativeHandle with size for this request type!
            : base(NativeMethods.GetSize(requestType))
        {
            if (initializer != null)
            {
                this.RequestType = requestType;
                this.watcherCallback = watcherCallback;
                this.closeOnCallback = closeOnCallback;

                try
                {
                    initializer(Handle);
                }
                catch
                {
                    FreeHandle(ref Handle);
                    throw;
                }
            }
            else throw new ArgumentException($"Initializer can't be null!");
        }

        void OnWatcherCallback(OperationException error)
        {
            try
            {
                if (error != null)
                {
                    Log.Error($"{RequestType} {Handle} error : {error.ErrorCode} {error.Name}: {error}");
                }

                watcherCallback?.Invoke(this, error);

                if (closeOnCallback)
                {
                    Dispose();
                }
            }
            catch (Exception exception)
            {
                Log.Error($"{RequestType} {nameof(OnWatcherCallback)} error: {exception}");
                throw;
            }
        }

        // C->C# callbacks need to be static and have MonoPInvokeCallback for
        // IL2CPP builds. avoids "System.NotSupportedException: To marshal a
        // managed method please add an attribute named 'MonoPInvokeCallback'"
        [MonoPInvokeCallback(typeof(uv_watcher_cb))]
        static void OnWatcherCallback(IntPtr handle, int status)
        {
            if (handle == IntPtr.Zero)
                return;

            // get from native lookup
            if (nativeLookup.TryGetValue(handle, out NativeHandle entry))
            {
                if (entry is WatcherRequest watcherRequest)
                {
                    OperationException error = status < 0 ? NativeMethods.CreateError((uv_err_code)status) : null;
                    watcherRequest.OnWatcherCallback(error);
                }
                else Log.Error($"WatcherRequest.OnWatcherCallback: unexpected lookup type: {entry.GetType()}");
            }
            // original had .? assuming it might be null. so don't log error.
            //else Log.Info($"WatcherRequest.OnWatcherCallback: no nativeLookup entry found for handle={handle}!");
        }
    }
}
