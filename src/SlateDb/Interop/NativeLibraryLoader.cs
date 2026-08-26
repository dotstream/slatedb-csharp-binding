using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SlateDb.Interop;

/// <summary>
/// Resolves the native slatedb_csharp_ffi library from the RID-specific
/// runtimes/{rid}/native folder shipped next to this assembly. This is
/// required because a plain ProjectReference (used by this repo's own
/// test/launcher projects) does not get the automatic RID-native-asset
/// flattening that a real PackageReference consumer gets from NuGet.
/// </summary>
internal static class NativeLibraryLoader
{
    private const string LibraryName = "slatedb_csharp_ffi";

    [ModuleInitializer]
    internal static void Initialize()
    {
        NativeLibrary.SetDllImportResolver(typeof(NativeLibraryLoader).Assembly, Resolve);
    }

    private static nint Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != LibraryName)
        {
            return nint.Zero;
        }

        var candidate = Path.Combine(AppContext.BaseDirectory, "runtimes", GetRuntimeIdentifier(), "native", GetNativeFileName());

        return File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out var handle)
            ? handle
            : nint.Zero;
    }

    private static string GetRuntimeIdentifier()
    {
        var arch = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "arm64" : "x64";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return $"osx-{arch}";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return $"win-{arch}";
        }

        return $"linux-{arch}";
    }

    private static string GetNativeFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return $"lib{LibraryName}.dylib";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return $"{LibraryName}.dll";
        }

        return $"lib{LibraryName}.so";
    }
}
