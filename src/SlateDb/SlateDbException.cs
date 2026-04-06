using SlateDb.Interop;

namespace SlateDb;

public sealed class SlateDbException : Exception
{
    private slatedb_result_t result;
    
    internal SlateDbException(slatedb_result_t result, String message)
        : base(message)
    {
        this.result = result;
    }   
}