using System.Reflection;
using System.Runtime.InteropServices;

namespace SlateDb.Interop;

internal static partial class NativeMethods
{
    static NativeMethods()
    {
        NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, DllImportResolver);
    }

    internal static IntPtr LoadDebugNativeLibrary()
    {
        return DllImportResolver(__DllName, typeof(NativeMethods).Assembly, null);
    }

    static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName == __DllName)
        {
            var name = libraryName;
            var ext = "";
            var prefix = "";
            var platform = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platform = "win";
                prefix = "";
                ext = ".dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platform = "osx";
                prefix = libraryName.StartsWith("lib") ? "" : "lib";
                ext = ".dylib";
            }
            else
            {
                platform = "linux";
                prefix = libraryName.StartsWith("lib") ? "" : "lib";
                ext = ".so";
            }

            var arch = RuntimeInformation.OSArchitecture switch
            {
                Architecture.Arm64 => "arm64",
                Architecture.X64 => "x64",
                Architecture.X86 => "x86",
                _ => throw new NotSupportedException(),
            };

            return NativeLibrary.Load($"runtimes/{platform}-{arch}/native/{prefix}{name}{ext}", assembly, searchPath);
        }

        return IntPtr.Zero;
    }
    
    [DllImport(__DllName, EntryPoint = "slatedb_db_builder_with_merge_operator", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern unsafe slatedb_result_t slatedb_db_builder_with_merge_operator(
        slatedb_db_builder_t* builder,
        SlatedbMergeOperatorFn merge_operator,
        SlateDbFreeMergeResultFn free_merge_result);
    
    
    [DllImport(__DllName, EntryPoint = "slatedb_wal_reader_list", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern unsafe slatedb_result_t slatedb_wal_reader_list(
        slatedb_wal_reader_t* reader,
        slatedb_range_t range,
        slatedb_wal_file_t*** out_files,
        ulong* out_count);
}