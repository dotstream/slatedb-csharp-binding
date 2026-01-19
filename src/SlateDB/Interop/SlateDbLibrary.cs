using System.Reflection;
using System.Runtime.InteropServices;

namespace SlateDB.Interop;

public static class SlateDbLibrary
{
    static object loadLockObj = new object();
    static bool isInitialized = false;
    
    public static bool IsInitialized
    {
        get
        {
            lock (loadLockObj)
            {
                return isInitialized;
            }
        }
    }
    
    internal static bool Initialize()
    {
        lock (loadLockObj)
        {
            if (isInitialized)
            {
                return false;
            }
            
            string libraryName = "slatedb_csharp_ffi";


            string libraryPath = Path.Combine(AppContext.BaseDirectory, "runtimes",
                RuntimeInformation.RuntimeIdentifier, "native", libraryName);
            
            IntPtr handle2 = NativeLibrary.
                Load(libraryName, Assembly.GetCallingAssembly(),
                DllImportSearchPath.ApplicationDirectory);

            // Load the library
            IntPtr handle = NativeLibrary.Load(libraryPath);
            if (handle == IntPtr.Zero)
            {
                throw new DllNotFoundException($"Could not load native library from {libraryPath}");
            }
            
            isInitialized = true;
            return true;
        }
    }
}