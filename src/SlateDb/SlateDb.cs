using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SlateDb.Interop;

[assembly: InternalsVisibleTo("SlateDbLauncher, PublicKey=0024000004800000140100000602000000240000525341310008000001000100CD8DFA6742EB7020886FA2384A5F25F846B365AFB78EE96A6FF2BD3B049D91AC36B9E5959F2EDE89481964D26D0DD0367C6B65A857F160107D54EAA2499CE900DAA13734D6CDC29B41A217E2BEAB3E646F9292D9B8B05B7E0F67E07201F266A894F8D6001C6B8402813FEA3E923FEE39F35692F127FC359F85F2B3CE6A01D1ABE5E7CD4AFB6EA3A732B50653DA44E33FD09DA67279D0B7F2623AA359321EA82C806E608DA118C5A64EA0F28CB5711D382825542C031C45CBC2EDCC60D51D938CDDBC11615CDA8D7C246F0794A027BE24A3B62BD57BDA372C9B9817E3117812032CC72DB9BE3720B300703FF57BEA90697F08234ED87226BC2EAE841F6EFC5EB5")]

namespace SlateDb;

public sealed unsafe class SlateDb : IDisposable
{
    private SafeHandle _handle;
    private bool _disposed;
    
    public SlateDb(string path)
    {
        #if DEBUG
        var ptr = NativeMethods.LoadDebugNativeLibrary();
        #endif 
        
        var status = NativeMethods.slatedb_open(path.ToPtr(), "memory://".ToPtr(), null);
        if (status.result.error != CSdbError.Success)
        {
            var message = Marshal.PtrToStringUTF8((IntPtr)status.result.message);
            throw new SlateDbException(status.result, message);
        }

        _handle = new SlateDbHandle(status.handle);
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _handle?.Dispose();
            _disposed = true;
        }
    }
}