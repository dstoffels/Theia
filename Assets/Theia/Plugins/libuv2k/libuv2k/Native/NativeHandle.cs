// IntPtr handle + Dispose logic so inheriting classes can reuse it.
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace libuv2k.Native
{
    public abstract class NativeHandle : IDisposable
    {
        public IntPtr Handle = IntPtr.Zero;

        internal bool IsValid => Handle != IntPtr.Zero;
        internal void SetInvalid() => Handle = IntPtr.Zero;

        // previously we used to set uv_stream_t.data user data field to the C#
        // TcpStream object via GCHandle.Alloc(TcpStream) and got it back from
        // uv_handle_t.data via GCHandle.FromIntPtr + casting.
        // this was way too complicated, error prone and slow.
        // => let's keep a global IntPtr<->TcpStream lookup instead.
        // => static state is WAY better than native allocated state!
        // (make sure to remove entries when disposing!)
        internal static Dictionary<IntPtr, NativeHandle> nativeLookup =
            new Dictionary<IntPtr, NativeHandle>();

        protected NativeHandle(int handleSize)
        {
            // allocate the handle
            Handle = Marshal.AllocCoTaskMem(handleSize);

            // associate C libuv handle with C# NativeHandle via lookup
            //   GCHandle gcHandle = GCHandle.Alloc(target, GCHandleType.Normal);
            //   ((uv_handle_t*)handle)->data = GCHandle.ToIntPtr(gcHandle);
            nativeLookup[Handle] = this;

            // previously we used to set handle.data to the allocated C#
            // NativeHandle. let's set handle.data to 0 just to be sure it's
            // not initialized with random memory that someone might mistake
            // for a TcpStream later.
            //
            // TOO RISKY: some NativeHandles are uv_handle_t and some are uv_req_t
            //            and it's not obvious that uv_req_t is always aligned
            //            with uv_handle_t so let's simply not clear for now.
            //            -> we aren't using .data anywhere anyway.
            //   ((uv_handle_t*)Handle)->data = IntPtr.Zero;
            //
            //Log.Info($"NativeHandle: allocated {Handle} of type {GetType()}");
        }

        // free the handle
        protected static void FreeHandle(ref IntPtr Handle)
        {
            if (Handle == IntPtr.Zero)
                return;

            // remove from native lookup and make sure it was actually still in
            // there. detects bugs like the one where we accidentally removed it
            // in libuv2k.Shutdown before letting FreeHandle do all the cleanup.
            if (!nativeLookup.Remove(Handle))
                Log.Error($"NativeHandle.FreeHandle: {Handle} was already removed from nativeLookup. This should have never happened!");

            // free memory
            Marshal.FreeCoTaskMem(Handle);
            Handle = IntPtr.Zero;
        }

        // CloseHandle calls ReleaseHandle by default, but some implementations
        // might need to call uv_close and call ReleaseHandle from the callback.
        protected virtual void CloseHandle() => FreeHandle(ref Handle);

        protected internal void Validate()
        {
            if (!IsValid)
            {
                throw new ObjectDisposedException($"{GetType()}");
            }
        }

        bool closed;
        void Dispose(bool disposing)
        {
            try
            {
                // keep the IsValid check just to be 100% sure we never EVER
                // operate on an invalid handle
                if (IsValid)
                {
                    // IMPORTANT: Dispose() might be called multiple times.
                    // BUT CloseHandle() should only ever be called ONCE!
                    // -> checking IsValid() is not enough, because if
                    //    CloseHandle() uses a uv_close callback then the handle
                    //    won't be invalid immediately, so another Dispose
                    //    call would allow us to call CloseHandle() again
                    // -> CloseHandle() would then fire off another uv_close
                    //    callback which will operate on invalid data as soon as
                    //    the first uv_close callback finishes
                    // -> this would crash both libuv and Unity with it
                    //
                    // so let's add a check to NEVER CALL IT TWICE!
                    //if (IsValid)
                    if (!closed)
                    {
                        // set the closed BEFORE we even call CloseHandle()
                        // just in case CloseHandle does any magic / circular calls.
                        // we can not ever risk calling it twice
                        closed = true;

                        //Log.Info($"Disposing {Handle} of type {GetType()} (Finalizer {!disposing})");
                        CloseHandle();

                        // IMPORTANT: we do not set invalid in any case.
                        // it's up to the CloseHandle() implementation,
                        // because some implementations might need a uv_close
                        // callback and only invalidate in there.
                    }
                    // log message for now. we can hide this log later.
                    else Log.Info($"NativeHandle of type {GetType()}: prevented redundant CloseHandle() call to avoid libuv operating on invalid data & potentially crashing Unity.");
                }
            }
            catch (Exception exception)
            {
                Log.Error($"{nameof(NativeHandle)} {Handle} error whilst closing handle: {exception}");

                // For finalizer, we cannot allow this to escape.
                if (disposing) throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NativeHandle()
        {
            // in Unity, destructor is called after domain reload (= after
            // changing a script and recompiling).
            // if we forgot to call CloseHandle, then Dispose() will close it
            // again.
            //
            // we ran the game / tests a long time ago and cleanup is only
            // attempted just now after domain reload. this works, but it's just
            // not very clean. we should call Dipose() when we are done with the
            // handle instead.
            //
            // for example, we might declare a 'public Loop loop = new Loop()'
            // but forget to Dispose it in Destroy(), which means that it would
            // take until domain reload -> destructor here for it to happen.
            //
            // in other words, let's show a warning that we forgot to call
            // Dispose (if still valid)
            if (IsValid)
            {
                Log.Warning($"Forgot to Dispose NativeHandle {Handle}. Disposing it now in destructor (likely after domain reload). Make sure to find out where a handle was created without Disposing it, and then Dispose it in all cases to avoid Disposing way later after domain reload, which could cause unexpected behaviour.");
            }

            Dispose(false);
        }
    }
}
