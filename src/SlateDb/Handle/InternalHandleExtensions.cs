using System.Runtime.InteropServices;
using SlateDb.Handle.Internal;

namespace SlateDb.Handle;

internal static class InternalHandleExtensions
{
    public static unsafe T* GetCSdbHandle<T>(this SafeHandle safeHandle)
        where T : struct
    {
        if (safeHandle is SlateDbHandle slateDbHandle)
        {
            var handle = slateDbHandle.GetCSdbHandle();
            return (T*)handle;
        }

        if (safeHandle is SlateDbReaderHandle slateDbReaderHandle)
        {
            var handle = slateDbReaderHandle.GetCSdbHandle();
            return (T*)handle;
        }

        if (safeHandle is SlateWalHandle slateWalHandle)
        {
            var handle = slateWalHandle.GetCSdbHandle();
            return (T*)handle;
        }

        if (safeHandle is SlateWalFileHandle slateWalFileHandle)
        {
            var handle = slateWalFileHandle.GetCSdbHandle();
            return (T*)handle;
        }
        

        return (T*)IntPtr.Zero;
    }
}