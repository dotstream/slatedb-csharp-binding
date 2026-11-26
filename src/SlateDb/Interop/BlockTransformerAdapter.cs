namespace SlateDb.Interop;

internal sealed class BlockTransformerAdapter(IBlockTransformer transformer) : BlockTransformer
{
    public byte[] Encode(byte[] data) => Call(transformer.Encode, data, "encode");

    public byte[] Decode(byte[] data) => Call(transformer.Decode, data, "decode");

    private static byte[] Call(Func<byte[], byte[]> fn, byte[] data, string operation)
    {
        try
        {
            return fn(data) ?? throw new InvalidOperationException($"Block transformer {operation} returned null");
        }
        catch (BlockTransformerCallbackException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BlockTransformerCallbackException.Failed(ex.Message);
        }
    }
}
