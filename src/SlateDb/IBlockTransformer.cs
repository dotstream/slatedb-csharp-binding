namespace SlateDb;

/// <summary>
/// Application-provided reversible transform of every SST block (data, index, filter and stats
/// alike), applied after compression and before the checksum, e.g. for encryption. Installed with
/// <see cref="SlateDbBuilder{K,V}.WithBlockTransformer"/>.
/// </summary>
/// <remarks>
/// <see cref="Decode"/> must invert <see cref="Encode"/> for every block the database wrote. SlateDB
/// records no transformer identity, key id or format version, so a block that needs one must carry
/// it inside its own bytes. Every reader of the database must install the same transformer.
/// Methods are called from a blocking thread pool, one call per block. Throwing an exception fails
/// the read or write of that block.
/// </remarks>
public interface IBlockTransformer
{
    /// <summary>Transforms a block before it is written.</summary>
    byte[] Encode(byte[] data);

    /// <summary>Reverses <see cref="Encode"/> on a block that was read.</summary>
    byte[] Decode(byte[] data);
}
