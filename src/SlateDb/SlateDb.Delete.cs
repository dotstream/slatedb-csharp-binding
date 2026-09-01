using SlateDb.Converter;
using SlateDb.Options;

namespace SlateDb;

public sealed partial class SlateDb<K,V>
{
    /// <summary>Deletes <paramref name="key"/>. Write-mode only.</summary>
    public void Delete(K key)
        => Delete(key, null);

    /// <summary>Deletes <paramref name="key"/> using custom write options. Write-mode only.</summary>
    public void Delete(K key, WriteOptions? options)
        => Delete(_keyConverter.ConvertClassToBytes(key), options);

    /// <summary>Deletes the raw key <paramref name="key"/>. Write-mode only.</summary>
    public void Delete(byte[] key)
        => Delete(key, null);

    /// <summary>Deletes the raw key <paramref name="key"/> using custom write options. Write-mode only.</summary>
    public void Delete(byte[]? key, WriteOptions? options)
    {
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        ArgumentNullException.ThrowIfNull(key);

        options ??= WriteOptions.Default;

        try
        {
            var handle = _dbHandle.DeleteWithOptions(key, Interop.OptionsConverters.ToInterop(options)).GetAwaiter().GetResult();
            Interop.UniffiHelpers.HandleWriteResult(handle, options.AwaitDurable);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Delete failed: {ex.Message}", ex);
        }
    }

    /// <summary>Deletes <paramref name="key"/> asynchronously. Write-mode only.</summary>
    public Task DeleteAsync(K key)
        => DeleteAsync(key, null);

    /// <summary>Deletes <paramref name="key"/> asynchronously using custom write options. Write-mode only.</summary>
    public Task DeleteAsync(K key, WriteOptions? options)
        => DeleteAsync(_keyConverter.ConvertClassToBytes(key), options);

    /// <summary>Deletes the raw key <paramref name="key"/> asynchronously. Write-mode only.</summary>
    public Task DeleteAsync(byte[] key)
        => DeleteAsync(key, null);

    /// <summary>Deletes the raw key <paramref name="key"/> asynchronously using custom write options. Write-mode only.</summary>
    public async Task DeleteAsync(byte[]? key, WriteOptions? options)
    {
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        ArgumentNullException.ThrowIfNull(key);

        options ??= WriteOptions.Default;

        try
        {
            var handle = await _dbHandle.DeleteWithOptions(key, Interop.OptionsConverters.ToInterop(options));
            await Interop.UniffiHelpers.HandleWriteResultAsync(handle, options.AwaitDurable);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Delete failed: {ex.Message}", ex);
        }
    }
}
