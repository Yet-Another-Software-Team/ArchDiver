using System.Runtime.InteropServices;

namespace ArchDiver.Parser.Infrastructure;

public static class PlatformHelper
{
    /// <summary>
    /// Gets the platform-specific library name (e.g., NAME.dll, libNAME.so, libNAME.dylib).
    /// </summary>
    public static string GetPlatformLibraryName(string baseName)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return $"{baseName}.dll";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return $"lib{baseName}.dylib";
        }
        else
        {
            // Default to Linux/Unix pattern
            return $"lib{baseName}.so";
        }
    }
}
