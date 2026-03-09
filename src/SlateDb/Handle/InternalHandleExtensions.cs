using System.Runtime.InteropServices;
using SlateDb.Handle.Internal;

namespace SlateDb.Handle;

internal static class InternalHandleExtensions
{
    public static T GetCSdbHandle<T>(this SafeHandle safeHandle)
        where T : struct
    {
        if (safeHandle is SlateDbHandle  slateDbHandle)
        {
            var handle = slateDbHandle.GetCSdbHandle();
            if (handle is T handleT)
                return handleT;
        } else if (safeHandle is SlateDbReaderHandle  slateDbReaderHandle)
        {
            var handle = slateDbReaderHandle.GetCSdbHandle();
            if(handle is T handleT)
                return handleT;
        }

        return default;
    }
}