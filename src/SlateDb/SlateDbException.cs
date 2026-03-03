using SlateDb.Interop;

namespace SlateDb;

public sealed class SlateDbException : Exception
{
    private CSdbResult result;
    
    internal SlateDbException(CSdbResult result, String message)
        : base(message)
    {
        this.result = result;
    }   
}