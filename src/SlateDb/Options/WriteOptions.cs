namespace SlateDb.Options;

/// <summary>
/// Options that control durability behavior for writes and commits.
/// </summary>
public record WriteOptions
{
    /// <summary>The default write options: waits for the write to become durable.</summary>
    public static WriteOptions Default => new() { AwaitDurable = true };

    /// <summary>Whether the call waits for the write to become durable before returning.</summary>
    public bool AwaitDurable { get; init; } = true;
}
