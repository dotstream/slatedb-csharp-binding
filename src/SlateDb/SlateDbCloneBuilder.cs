using SlateDb.Admin;
using SlateDb.Configuration;

namespace SlateDb;

/// <summary>
/// Builder for cloning a database from an existing source, created via
/// <see cref="SlateDbAdmin.CreateCloneBuilderFromSource"/>.
/// </summary>
public sealed class SlateDbCloneBuilder
{
    private readonly Interop.CloneBuilder _handle;

    internal SlateDbCloneBuilder(Interop.CloneBuilder handle)
    {
        _handle = handle;
    }

    /// <summary>Sets the path the clone is created at.</summary>
    public SlateDbCloneBuilder WithClonePath(string clonePath)
    {
        _handle.WithClonePath(clonePath);
        return this;
    }

    /// <summary>Sets the source database and state the clone is created from.</summary>
    public SlateDbCloneBuilder WithSource(CloneSourceSpec source)
    {
        _handle.WithSource(Interop.AdminConverters.ToInterop(source));
        return this;
    }

    /// <summary>Sets the object store the clone is created in.</summary>
    public SlateDbCloneBuilder WithObjectConfiguration(AbstractSlateDbConfig configuration)
    {
        using var objectStore = Interop.UniffiHelpers.CreateObjectStore(configuration);
        _handle.WithObjectStore(objectStore);
        return this;
    }

    /// <summary>Uses a separate object store for the clone's WAL.</summary>
    public SlateDbCloneBuilder WithWalObjectConfiguration(AbstractSlateDbConfig configuration)
    {
        using var walObjectStore = Interop.UniffiHelpers.CreateObjectStore(configuration);
        _handle.WithWalObjectStore(walObjectStore);
        return this;
    }

    /// <summary>Restricts the visible keys copied from the source to <paramref name="projectionRange"/>.</summary>
    public SlateDbCloneBuilder WithProjectionRange(KeyRange? projectionRange)
    {
        _handle.WithProjectionRange(projectionRange is null ? null : Interop.AdminConverters.ToInterop(projectionRange));
        return this;
    }

    /// <summary>Sets the seed used for SlateDB's internal random number generation.</summary>
    public SlateDbCloneBuilder WithSeed(ulong seed)
    {
        _handle.WithSeed(seed);
        return this;
    }

    /// <summary>Runs the clone operation and consumes this builder.</summary>
    public void Build() => BuildAsync().GetAwaiter().GetResult();

    /// <summary>Runs the clone operation asynchronously and consumes this builder.</summary>
    public async Task BuildAsync()
    {
        try
        {
            await _handle.Build();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"CloneBuilder.Build failed: {ex.Message}", ex);
        }
    }
}
