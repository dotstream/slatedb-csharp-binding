using System.Runtime.InteropServices;

namespace SlateDb;

public static class SlateDbExtensions
{
    public static unsafe byte* ToPtr(this String content)
    {
        var pathPtr = Marshal.StringToHGlobalAnsi(content);
        return (byte*)pathPtr;
    }
}