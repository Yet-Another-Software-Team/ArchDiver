using System;
using System.Runtime.InteropServices;

namespace ArchDiver.Parser.Infrastructure;

/// <summary>
/// P/Invoke bindings for the TreeSitterLanguagePack core library (ts_pack_core_ffi).
/// </summary>
internal static partial class TslpNative
{
    private const string LibName = "ts_pack_core_ffi";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr LanguageFunction();

    [DllImport(LibName, EntryPoint = "ts_pack_get_language", CharSet = CharSet.Ansi)]
    public static extern IntPtr GetLanguage(string name);

    [DllImport(LibName, EntryPoint = "ts_pack_last_error_code")]
    public static extern int LastErrorCode();

    [DllImport(LibName, EntryPoint = "ts_pack_cache_dir")]
    public static extern IntPtr CacheDir();

    [DllImport(LibName, EntryPoint = "ts_pack_last_error_context")]
    public static extern IntPtr LastErrorContext();

    /// <summary>
    /// Gets the last error message from the native core.
    /// </summary>
    public static string GetLastError()
    {
        IntPtr ptr = LastErrorContext();
        if (ptr == IntPtr.Zero) return "Unknown error";
        string? msg = Marshal.PtrToStringUTF8(ptr);
        return msg ?? "Unknown error";
    }
}
