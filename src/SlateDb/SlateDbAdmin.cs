using SlateDb.Admin;

namespace SlateDb;

/// <summary>
/// Administrative read/query handle for SlateDB, created via <see cref="SlateDbAdminBuilder"/>.
/// Exposes manifest, compaction, checkpoint, and garbage-collection introspection and control
/// that isn't available through <see cref="SlateDb{K,V}"/>.
/// </summary>
public sealed class SlateDbAdmin : IDisposable
{
    private readonly Interop.Admin _handle;
    private bool _disposed;

    internal SlateDbAdmin(Interop.Admin handle)
    {
        _handle = handle;
    }

    /// <summary>Reads a specific manifest by ID, or the latest when <paramref name="id"/> is <c>null</c>.</summary>
    public VersionedManifest? ReadManifest(ulong? id = null) => ReadManifestAsync(id).GetAwaiter().GetResult();

    /// <summary>Reads a specific manifest by ID, or the latest when <paramref name="id"/> is <c>null</c>, asynchronously.</summary>
    public async Task<VersionedManifest?> ReadManifestAsync(ulong? id = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var manifest = await _handle.ReadManifest(id);
            return manifest is null ? null : Interop.AdminConverters.ToPublic(manifest);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Admin.ReadManifest failed: {ex.Message}", ex);
        }
    }

    /// <summary>Lists manifests inside the half-open ID range <c>[from, to)</c>.</summary>
    public IReadOnlyList<VersionedManifest> ListManifests(ulong? from = null, ulong? to = null) =>
        ListManifestsAsync(from, to).GetAwaiter().GetResult();

    /// <summary>Lists manifests inside the half-open ID range <c>[from, to)</c>, asynchronously.</summary>
    public async Task<IReadOnlyList<VersionedManifest>> ListManifestsAsync(ulong? from = null, ulong? to = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var manifests = await _handle.ListManifests(from, to);
            return manifests.Select(Interop.AdminConverters.ToPublic).ToList();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Admin.ListManifests failed: {ex.Message}", ex);
        }
    }

    /// <summary>Reads a specific compactions file by ID, or the latest when <paramref name="id"/> is <c>null</c>.</summary>
    public VersionedCompactions? ReadCompactions(ulong? id = null) => ReadCompactionsAsync(id).GetAwaiter().GetResult();

    /// <summary>Reads a specific compactions file by ID, or the latest when <paramref name="id"/> is <c>null</c>, asynchronously.</summary>
    public async Task<VersionedCompactions?> ReadCompactionsAsync(ulong? id = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var compactions = await _handle.ReadCompactions(id);
            return compactions is null ? null : Interop.AdminConverters.ToPublic(compactions);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Admin.ReadCompactions failed: {ex.Message}", ex);
        }
    }

    /// <summary>Reads a compaction by ULID string from a specific or latest compactions file.</summary>
    public Compaction? ReadCompaction(string compactionId, ulong? compactionsId = null) =>
        ReadCompactionAsync(compactionId, compactionsId).GetAwaiter().GetResult();

    /// <summary>Reads a compaction by ULID string from a specific or latest compactions file, asynchronously.</summary>
    public async Task<Compaction?> ReadCompactionAsync(string compactionId, ulong? compactionsId = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var compaction = await _handle.ReadCompaction(compactionId, compactionsId);
            return compaction is null ? null : Interop.AdminConverters.ToPublic(compaction);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Admin.ReadCompaction failed: {ex.Message}", ex);
        }
    }

    /// <summary>Reads the latest compactor state view.</summary>
    public CompactorStateView ReadCompactorStateView() => ReadCompactorStateViewAsync().GetAwaiter().GetResult();

    /// <summary>Reads the latest compactor state view, asynchronously.</summary>
    public async Task<CompactorStateView> ReadCompactorStateViewAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var view = await _handle.ReadCompactorStateView();
            return Interop.AdminConverters.ToPublic(view);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Admin.ReadCompactorStateView failed: {ex.Message}", ex);
        }
    }

    /// <summary>Generates a compaction from <paramref name="spec"/> and submits it.</summary>
    public Compaction SubmitCompaction(CompactionSpec spec) => SubmitCompactionAsync(spec).GetAwaiter().GetResult();

    /// <summary>Generates a compaction from <paramref name="spec"/> and submits it, asynchronously.</summary>
    public async Task<Compaction> SubmitCompactionAsync(CompactionSpec spec)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var compaction = await _handle.SubmitCompaction(Interop.AdminConverters.ToInterop(spec));
            return Interop.AdminConverters.ToPublic(compaction);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Admin.SubmitCompaction failed: {ex.Message}", ex);
        }
    }

    /// <summary>Lists compactions files inside the half-open ID range <c>[from, to)</c>.</summary>
    public IReadOnlyList<VersionedCompactions> ListCompactions(ulong? from = null, ulong? to = null) =>
        ListCompactionsAsync(from, to).GetAwaiter().GetResult();

    /// <summary>Lists compactions files inside the half-open ID range <c>[from, to)</c>, asynchronously.</summary>
    public async Task<IReadOnlyList<VersionedCompactions>> ListCompactionsAsync(ulong? from = null, ulong? to = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var compactions = await _handle.ListCompactions(from, to);
            return compactions.Select(Interop.AdminConverters.ToPublic).ToList();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Admin.ListCompactions failed: {ex.Message}", ex);
        }
    }

    /// <summary>Lists checkpoints, optionally filtering by exact name.</summary>
    public IReadOnlyList<Checkpoint> ListCheckpoints(string? nameFilter = null) =>
        ListCheckpointsAsync(nameFilter).GetAwaiter().GetResult();

    /// <summary>Lists checkpoints, optionally filtering by exact name, asynchronously.</summary>
    public async Task<IReadOnlyList<Checkpoint>> ListCheckpointsAsync(string? nameFilter = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var checkpoints = await _handle.ListCheckpoints(nameFilter);
            return checkpoints.Select(Interop.AdminConverters.ToPublic).ToList();
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Admin.ListCheckpoints failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Runs the garbage collector once with the provided options. When <paramref name="options"/>
    /// is <c>null</c>, SlateDB's default garbage collector options are used.
    /// </summary>
    public void RunGcOnce(GarbageCollectorOptions? options = null) => RunGcOnceAsync(options).GetAwaiter().GetResult();

    /// <summary>Runs the garbage collector once with the provided options, asynchronously.</summary>
    public async Task RunGcOnceAsync(GarbageCollectorOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            await _handle.RunGcOnce(options is null ? null : Interop.AdminConverters.ToInterop(options));
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Admin.RunGcOnce failed: {ex.Message}", ex);
        }
    }

    /// <summary>Looks up a timestamp for the provided sequence number.</summary>
    public long? GetTimestampForSequence(ulong seq, bool roundUp) =>
        GetTimestampForSequenceAsync(seq, roundUp).GetAwaiter().GetResult();

    /// <summary>Looks up a timestamp for the provided sequence number, asynchronously.</summary>
    public async Task<long?> GetTimestampForSequenceAsync(ulong seq, bool roundUp)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            return await _handle.GetTimestampForSequence(seq, roundUp);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Admin.GetTimestampForSequence failed: {ex.Message}", ex);
        }
    }

    /// <summary>Looks up a sequence number for the provided Unix UTC timestamp seconds.</summary>
    public ulong? GetSequenceForTimestamp(long timestampSecs, bool roundUp) =>
        GetSequenceForTimestampAsync(timestampSecs, roundUp).GetAwaiter().GetResult();

    /// <summary>Looks up a sequence number for the provided Unix UTC timestamp seconds, asynchronously.</summary>
    public async Task<ulong?> GetSequenceForTimestampAsync(long timestampSecs, bool roundUp)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            return await _handle.GetSequenceForTimestamp(timestampSecs, roundUp);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Admin.GetSequenceForTimestamp failed: {ex.Message}", ex);
        }
    }

    /// <summary>Creates a checkpoint of the database using the provided options.</summary>
    public CheckpointCreateResult CreateDetachedCheckpoint(CheckpointOptions options) =>
        CreateDetachedCheckpointAsync(options).GetAwaiter().GetResult();

    /// <summary>Creates a checkpoint of the database using the provided options, asynchronously.</summary>
    public async Task<CheckpointCreateResult> CreateDetachedCheckpointAsync(CheckpointOptions options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var result = await _handle.CreateDetachedCheckpoint(Interop.AdminConverters.ToInterop(options));
            return Interop.AdminConverters.ToPublic(result);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Admin.CreateDetachedCheckpoint failed: {ex.Message}", ex);
        }
    }

    /// <summary>Refreshes the lifetime of an existing checkpoint.</summary>
    public void RefreshCheckpoint(string id, ulong? lifetimeMs = null) =>
        RefreshCheckpointAsync(id, lifetimeMs).GetAwaiter().GetResult();

    /// <summary>Refreshes the lifetime of an existing checkpoint, asynchronously.</summary>
    public async Task RefreshCheckpointAsync(string id, ulong? lifetimeMs = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            await _handle.RefreshCheckpoint(id, lifetimeMs);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Admin.RefreshCheckpoint failed: {ex.Message}", ex);
        }
    }

    /// <summary>Deletes the checkpoint with the specified id.</summary>
    public void DeleteCheckpoint(string id) => DeleteCheckpointAsync(id).GetAwaiter().GetResult();

    /// <summary>Deletes the checkpoint with the specified id, asynchronously.</summary>
    public async Task DeleteCheckpointAsync(string id)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            await _handle.DeleteCheckpoint(id);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Admin.DeleteCheckpoint failed: {ex.Message}", ex);
        }
    }

    /// <summary>Creates a builder for cloning a database from <paramref name="source"/>.</summary>
    public SlateDbCloneBuilder CreateCloneBuilderFromSource(CloneSourceSpec source)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var builder = _handle.CreateCloneBuilderFromSource(Interop.AdminConverters.ToInterop(source));
            return new SlateDbCloneBuilder(builder);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Admin.CreateCloneBuilderFromSource failed: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _handle.Dispose();
            _disposed = true;
        }
    }
}
