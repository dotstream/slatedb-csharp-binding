using SlateDb.Converter;

namespace SlateDb;

internal static class SlateDbAsyncScan
{
    public static async IAsyncEnumerable<SlateDbKeyValue<K, V>> Enumerate<K, V>(
        Interop.DbIterator iterator,
        ISlateDbConverter<K>? keyConverter,
        ISlateDbConverter<V>? valueConverter,
        string operation)
        where K : class
        where V : class
    {
        using (iterator)
        {
            while (true)
            {
                var kv = await NextOrThrow(iterator, operation);
                if (kv == null)
                    yield break;

                yield return new SlateDbKeyValue<K, V>(
                    keyConverter.ConvertBytesToClass(kv.Key),
                    valueConverter.ConvertBytesToClass(kv.Value));
            }
        }
    }

    private static async Task<Interop.KeyValue?> NextOrThrow(Interop.DbIterator iterator, string operation)
    {
        try
        {
            return await iterator.Next();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"{operation} iterator failed: {ex.Message}", ex);
        }
    }
}
