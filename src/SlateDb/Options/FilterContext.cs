namespace SlateDb.Options;

/// <summary>
/// Opaque caller-supplied context forwarded to custom filter policies at
/// query time via <see cref="ReadOptions.FilterContext"/> or
/// <see cref="ScanOptions.FilterContext"/>. Built-in policies (including the
/// bloom filter) ignore it.
/// </summary>
public abstract record FilterContext
{
    private FilterContext() { }

    /// <summary>A fixed 64-byte inline payload.</summary>
    public sealed record Bytes(byte[] Payload) : FilterContext;
}
