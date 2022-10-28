// IMPORTANT: call loop.Dispose in OnDestroy to avoid a Unity Editor crash where
//            changing and recompiling a script would cause the disposal to
//            access null pointers and crash the Editor.
using AOT;
using System;
using libuv2k.Native;

namespace libuv2k
{
    public sealed class Loop : NativeHandle
    {
        static readonly uv_walk_cb WalkCallback = OnWalkCallback;

        public Loop()
            // allocate base NativeHandle with size of loop
            : base(NativeMethods.GetLoopSize())
        {
            try
            {
                NativeMethods.InitializeLoop(Handle);
            }
            catch
            {
                FreeHandle(ref Handle);
                throw;
            }
        }

        public bool IsAlive => IsValid && NativeMethods.IsLoopAlive(Handle);

        // check to indicate if a uv_run is in progress
        // needed to detect and avoid a crash, see CloseHandle()!
        bool uv_run_inprogress;
        public int Run(uv_run_mode mode)
        {
            Validate();
            uv_run_inprogress = true;
            int result = NativeMethods.RunLoop(Handle, mode);
            uv_run_inprogress = false;
            return result;
        }

        public void Stop()
        {
            Validate();
            NativeMethods.StopLoop(Handle);
        }

        protected override void CloseHandle()
        {
            // prevent Unity deadlock (in debug mode) / crash (in release mode)
            // caused like this:
            //   Transport.Update()
            //     client.uv_run()
            //       client.OnDisconnect.Invoke()
            //         OnDisconnect:
            //           loop.Dispose()
            //             loop.CloseHandle()
            //               ... loop is now gone and can't be accessed anymore
            //     client.uv_run() continues with the loop that is gone
            //       libuv native C assert crashes Unity
            if (uv_run_inprogress)
            {
                // note: throwing an exception would be cleaner but then
                // everything is going to stop functioning and run_in_progress
                // would never be reset etc.
                // TODO unless we reset it here?
                Log.Error("Loop.CloseHandle called during uv_run. Check stack trace and make sure to never call loop.Dispose from within a uv_run. For example, don't call loop.Dispose() in client.OnDisconnect which would be called from uv_run after a disconnect message.");
                return;
            }

            // how to properly close a libuv loop:
            // https://stackoverflow.com/questions/25615340/closing-libuv-handles-correctly
            //
            //   libuv is not done with a handle until it's close callback is
            //   called. That is the exact moment when you can free the handle.
            //
            //   I see you call uv_loop_close, but you don't check for the
            //   return value. If there are still pending handles, it will
            //   return UV_EBUSY, so you should check for that.
            //
            //   If you want to close a loop and close all handles, you need
            //   to do the following:
            //
            //   - Use uv_stop to stop the loop
            //   - Use uv_walk and call uv_close on all handles which are not
            //     closing
            //   - Run the loop again with uv_run so all close callbacks are
            //     called and you can free the memory in the callbacks
            //   - Call uv_loop_close, it should return 0 now
            //     if it returns UV_EBUSY then there are still open handles.
            //

            Log.Info($"Loop.CloseHandle: calling uv_walk for Handle={Handle}");
            NativeMethods.WalkLoop(Handle, WalkCallback);

            Log.Info($"Loop {Handle} running default to call close callbacks");
            NativeMethods.RunLoop(Handle, uv_run_mode.UV_RUN_DEFAULT);

            Log.Info($"Loop {Handle} close loop");
            int error = NativeMethods.CloseLoop(Handle);
            if (error == 0)
            {
                // free base NativeHandle only if loop was closed successfully.
                // we do not want access violations or crashes otherwise.
                FreeHandle(ref Handle);
            }
            else Log.Error($"Loop {Handle} close failed with error {error}");
        }

        // C->C# callbacks need to be static and have MonoPInvokeCallback for
        // IL2CPP builds. avoids "System.NotSupportedException: To marshal a
        // managed method please add an attribute named 'MonoPInvokeCallback'"
        [MonoPInvokeCallback(typeof(uv_walk_cb))]
        static void OnWalkCallback(IntPtr handle, IntPtr loopHandle)
        {
            if (handle == IntPtr.Zero)
                return;

            try
            {
                // look up the C# TcpStream for this native handle
                if (nativeLookup.TryGetValue(handle, out NativeHandle entry))
                {
                    if (entry is TcpStream stream)
                    {
                        stream.Dispose();
                        //Log.Info($"Loop {loopHandle} walk callback disposed {handle} with TcpStream={stream}");

                        // TODO maybe we should set nativeLookup[handle] = null
                        // since we just disposed it?
                        // but one step after another. original didn't set anything
                        // to null either.
                    }
                    else Log.Error($"Loop.OnWalkCallback: unexpected lookup type: {entry.GetType()}");
                }
                // original used ?.Dispose()
                // probably it's okay if null.
                //else Log.Warning($"TcpStream.OnAllocateCallback: nativeLookup no entry found for handle={handle}");
            }
            catch (Exception exception)
            {
                Log.Warning($"Loop {loopHandle} Walk callback attempt to close handle {handle} failed. {exception}");
            }
        }
    }
}
