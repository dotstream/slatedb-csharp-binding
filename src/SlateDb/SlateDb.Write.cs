using SlateDb.Converter;
using SlateDb.Options;

namespace SlateDb;

public sealed partial class SlateDb<K,V>
{
    /// <summary>Inserts or overwrites <paramref name="value"/> for <paramref name="key"/>, using custom put and write options. Write-mode only.</summary>
    public void Put(K key, V value, PutOptions putOptions, WriteOptions writeOptions)
        => Put(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), putOptions,  writeOptions);

    /// <summary>Inserts or overwrites <paramref name="value"/> for <paramref name="key"/>. Write-mode only.</summary>
    public void Put(K key, V value)
        => Put(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), null, null);

    /// <summary>Inserts or overwrites raw bytes for <paramref name="key"/>, using custom put and write options. Write-mode only.</summary>
    public void Put(byte[]? key, byte[]? value, PutOptions? putOptions, WriteOptions? writeOptions)
    {
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        putOptions ??= PutOptions.NoExpiry;
        writeOptions ??= WriteOptions.Default;

        try
        {
            var handle = _dbHandle.PutWithOptions(
                key, value,
                Interop.OptionsConverters.ToInterop(putOptions),
                Interop.OptionsConverters.ToInterop(writeOptions)).GetAwaiter().GetResult();
            Interop.UniffiHelpers.HandleWriteResult(handle, writeOptions.AwaitDurable);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Put failed: {ex.Message}", ex);
        }
    }

    /// <summary>Inserts or overwrites <paramref name="value"/> for <paramref name="key"/> asynchronously, using custom put and write options. Write-mode only.</summary>
    public Task PutAsync(K key, V value, PutOptions putOptions, WriteOptions writeOptions)
        => PutAsync(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), putOptions, writeOptions);

    /// <summary>Inserts or overwrites <paramref name="value"/> for <paramref name="key"/> asynchronously. Write-mode only.</summary>
    public Task PutAsync(K key, V value)
        => PutAsync(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), null, null);

    /// <summary>Inserts or overwrites raw bytes for <paramref name="key"/> asynchronously, using custom put and write options. Write-mode only.</summary>
    public async Task PutAsync(byte[]? key, byte[]? value, PutOptions? putOptions, WriteOptions? writeOptions)
    {
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        putOptions ??= PutOptions.NoExpiry;
        writeOptions ??= WriteOptions.Default;

        try
        {
            var handle = await _dbHandle.PutWithOptions(
                key, value,
                Interop.OptionsConverters.ToInterop(putOptions),
                Interop.OptionsConverters.ToInterop(writeOptions));
            await Interop.UniffiHelpers.HandleWriteResultAsync(handle, writeOptions.AwaitDurable);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Put failed: {ex.Message}", ex);
        }
    }
}
