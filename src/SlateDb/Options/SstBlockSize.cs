namespace SlateDb.Options;

/// <summary>
/// Block size used for newly written SSTable blocks.
/// </summary>
public enum SstBlockSize : byte
{
    /// <summary>1 KiB blocks.</summary>
    Block1KB = 0,

    /// <summary>2 KiB blocks.</summary>
    Block2KB = 1,

    /// <summary>4 KiB blocks.</summary>
    Block4KB = 2,

    /// <summary>8 KiB blocks.</summary>
    Block8KB = 3,

    /// <summary>16 KiB blocks.</summary>
    Block16KB = 4,

    /// <summary>32 KiB blocks.</summary>
    Block32KB = 5,

    /// <summary>64 KiB blocks.</summary>
    Block64KB = 6,
}
