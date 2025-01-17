﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace libuv2k.Native
{
    enum uv_err_code
    {
        UV_OK = 0,
        UV_E2BIG,
        UV_EACCES,
        UV_EADDRINUSE,
        UV_EADDRNOTAVAIL,
        UV_EAFNOSUPPORT,
        UV_EAGAIN,
        UV_EAI_ADDRFAMILY,
        UV_EAI_AGAIN,
        UV_EAI_BADFLAGS,
        UV_EAI_BADHINTS,
        UV_EAI_CANCELED,
        UV_EAI_FAIL,
        UV_EAI_FAMILY,
        UV_EAI_MEMORY,
        UV_EAI_NODATA,
        UV_EAI_NONAME,
        UV_EAI_OVERFLOW,
        UV_EAI_PROTOCOL,
        UV_EAI_SERVICE,
        UV_EAI_SOCKTYPE,
        UV_EALREADY,
        UV_EBADF,
        UV_EBUSY,
        UV_ECANCELED,
        UV_ECHARSET,
        UV_ECONNABORTED,
        UV_ECONNREFUSED,
        UV_ECONNRESET = -104, // -104 on linux
        UV_EDESTADDRREQ,
        UV_EEXIST,
        UV_EFAULT,
        UV_EFBIG,
        UV_EHOSTUNREACH,
        UV_EINTR,
        UV_EINVAL,
        UV_EIO,
        UV_EISCONN,
        UV_EISDIR,
        UV_ELOOP,
        UV_EMFILE,
        UV_EMSGSIZE,
        UV_ENAMETOOLONG,
        UV_ENETDOWN,
        UV_ENETUNREACH,
        UV_ENFILE,
        UV_ENOBUFS,
        UV_ENODEV,
        UV_ENOENT,
        UV_ENOMEM,
        UV_ENONET,
        UV_ENOPROTOOPT,
        UV_ENOSPC,
        UV_ENOSYS,
        UV_ENOTCONN,
        UV_ENOTDIR,
        UV_ENOTEMPTY,
        UV_ENOTSOCK,
        UV_ENOTSUP,
        UV_EPERM,
        UV_EPIPE,
        UV_EPROTO,
        UV_EPROTONOSUPPORT,
        UV_EPROTOTYPE,
        UV_ERANGE,
        UV_EROFS,
        UV_ESHUTDOWN,
        UV_ESPIPE,
        UV_ESRCH,
        UV_ETIMEDOUT,
        UV_ETXTBSY,
        UV_EXDEV,
        UV_UNKNOWN,
        UV_EOF = -4095,
        UV_ENXIO,
        UV_EMLINK,
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void uv_close_cb(IntPtr conn);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void uv_watcher_cb(IntPtr watcher, int status);

    static partial class NativeMethods
    {
        const string LibraryName = "libuv";

        internal static bool IsHandleActive(IntPtr handle) =>
            handle != IntPtr.Zero && uv_is_active(handle) != 0;

        internal static void CloseHandle(IntPtr handle, uv_close_cb callback)
        {
            if (handle == IntPtr.Zero || callback == null)
            {
                return;
            }

            int result = uv_is_closing(handle);
            if (result == 0)
            {
                uv_close(handle, callback);
            }
        }

        internal static bool IsHandleClosing(IntPtr handle) =>
            handle != IntPtr.Zero && uv_is_closing(handle) != 0;

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        static extern void uv_close(IntPtr handle, uv_close_cb close_cb);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        static extern int uv_is_closing(IntPtr handle);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        static extern int uv_is_active(IntPtr handle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ThrowIfError(int code)
        {
            if (code < 0)
            {
                ThrowOperationException((uv_err_code)code);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowOperationException(uv_err_code error) => throw CreateError(error);

        internal static OperationException CreateError(uv_err_code error)
        {
            string name = GetErrorName(error);
            string description = GetErrorDescription(error);
            return new OperationException((int)error, name, description);
        }

        static string GetErrorDescription(uv_err_code code)
        {
            IntPtr ptr = uv_strerror(code);
            return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
        }

        static string GetErrorName(uv_err_code code)
        {
            IntPtr ptr = uv_err_name(code);
            return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr uv_strerror(uv_err_code err);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr uv_err_name(uv_err_code err);

        internal static Version GetVersion()
        {
            uint version = uv_version();
            int major = (int)(version & 0xFF0000) >> 16;
            int minor = (int)(version & 0xFF00) >> 8;
            int patch = (int)(version & 0xFF);

            return new Version(major, minor, patch);
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        static extern uint uv_version();
    }
}
