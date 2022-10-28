using System;
using System.Collections.Generic;
using libuv2k.Native;

namespace libuv2k
{
    public static class libuv2k
    {
        static int initialized = 0;

        // Mirror/DOTSNET might have multiple worlds.
        // calling shutdown in one world's OnDestroy should not shut libuv down
        // while the other world's libuv is still running and destroy hasn't
        // been called yet.
        // => the easiest solution is to count initialize/shutdowns and do
        //    nothing if there is still an instance running.
        // => note that we are merely working around static state here. but
        //    we need NativeHandle.nativeLookup to be static because libuv's
        //    native callbacks call static functions.
        // => this is fine :)
        public static void Initialize()
        {
            ++initialized;
        }

        // one global Shutdown method to be called after using libuv2k, so the
        // user doesn't need to dispose pool entires manually, etc.
        public static void Shutdown()
        {
            // see Initialize() comment: if libuv is running in multiple worlds,
            // only call Shutdown when the last world is shut down so we don't
            // shut libuv down while it's still used in another world (DOTSNET)!
            --initialized;
            if (initialized > 0) return;
            if (initialized < 0) Log.Error("libuv2k.Shutdown was called without a prior libuv2k.Initialize call!");

            Log.Info("libuv2k.Shutdown!");

            // it's very important that we dispose every WriteRequest in our
            // Pool. otherwise it will take until domain reload for the
            // NativeHandle destructor to be called to clean them up, which
            // wouldn't be very clean.
            TcpStream.WriteRequestPool.Clear();

            // make sure all native handles were disposed.
            // if this isn't empty then we forgot to dispose something somewhere.
            //
            // IMPORTANT: do this AFTER disposing all WriteRequestPool entries.
            //            and after every other cleanup.
            foreach (KeyValuePair<IntPtr, NativeHandle> kvp in NativeHandle.nativeLookup)
            {
                Log.Error($"NativeHandle {kvp.Key} of type {kvp.Value.GetType()} has not been Disposed. Check the code to see where a Dispose call is missing!");
            }

            // FIX: we still clear whatever was left in nativeLookup!
            // => yes, it was not disposed and we did log an error
            // => but even if next libuv session would do everything right,
            //    keeping the old values would still log errors again and be
            //    extremely confusing.
            // => all tests would keep failing until domain reload.
            NativeHandle.nativeLookup.Clear();
        }
    }
}