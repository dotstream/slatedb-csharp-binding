using SlateDb.Converter;
using SlateDb.Options;

namespace SlateDb;

public sealed partial class SlateDb<K,V>
{
    /// <summary>Reads the raw stored bytes for <paramref name="key"/>.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public byte[]? GetRawBytes(K key) => GetRawBytes(key, null);

    /// <summary>Reads the raw stored bytes for <paramref name="key"/> using custom read options.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public byte[]? GetRawBytes(K key, ReadOptions? options)
        => GetRawBytes(_keyConverter.ConvertClassToBytes(key), options);

    /// <summary>Reads the current value for <paramref name="key"/>.</summary>
    /// <returns>The deserialized value, or <c>null</c> if the key does not exist.</returns>
    public V? Get(K key)
        => Get(key, null);

    /// <summary>Reads the current value for <paramref name="key"/> using custom read options.</summary>
    /// <returns>The deserialized value, or <c>null</c> if the key does not exist.</returns>
    public V? Get(K key, ReadOptions? options)
    {
        var bytes = GetRawBytes(_keyConverter.ConvertClassToBytes(key), options);
        return bytes is null ? null : _valueConverter.ConvertBytesToClass(bytes);
    }

    /// <summary>Reads the raw stored bytes for <paramref name="key"/> using custom read options.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public byte[]? GetRawBytes(byte[]? key, ReadOptions? options)
    {
        options ??= ReadOptions.Default;

        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            var interopOptions = Interop.OptionsConverters.ToInterop(options);

            if (_mode == SlateDbMode.Readwrite)
            {
                if (_dbHandle == null)
                    throw new SlateDbException("Database handle is null");

                return _dbHandle.GetWithOptions(key, interopOptions).GetAwaiter().GetResult();
            }
            else
            {
                if (_readerHandle == null)
                    throw new SlateDbException("Reader handle is null");

                return _readerHandle.GetWithOptions(key, interopOptions).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Get failed: {ex.Message}", ex);
        }
    }

    /// <summary>Reads the raw stored bytes for <paramref name="key"/> asynchronously.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public Task<byte[]?> GetRawBytesAsync(K key) => GetRawBytesAsync(key, null);

    /// <summary>Reads the raw stored bytes for <paramref name="key"/> asynchronously, using custom read options.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public Task<byte[]?> GetRawBytesAsync(K key, ReadOptions? options)
        => GetRawBytesAsync(_keyConverter.ConvertClassToBytes(key), options);

    /// <summary>Reads the current value for <paramref name="key"/> asynchronously.</summary>
    /// <returns>The deserialized value, or <c>null</c> if the key does not exist.</returns>
    public async Task<V?> GetAsync(K key) => await GetAsync(key, null);

    /// <summary>Reads the current value for <paramref name="key"/> asynchronously, using custom read options.</summary>
    /// <returns>The deserialized value, or <c>null</c> if the key does not exist.</returns>
    public async Task<V?> GetAsync(K key, ReadOptions? options)
    {
        var bytes = await GetRawBytesAsync(_keyConverter.ConvertClassToBytes(key), options);
        return bytes is null ? null : _valueConverter.ConvertBytesToClass(bytes);
    }

    /// <summary>Reads the raw stored bytes for <paramref name="key"/> asynchronously, using custom read options.</summary>
    /// <returns>The raw value bytes, or <c>null</c> if the key does not exist.</returns>
    public async Task<byte[]?> GetRawBytesAsync(byte[]? key, ReadOptions? options)
    {
        options ??= ReadOptions.Default;

        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            var interopOptions = Interop.OptionsConverters.ToInterop(options);

            if (_mode == SlateDbMode.Readwrite)
            {
                if (_dbHandle == null)
                    throw new SlateDbException("Database handle is null");

                return await _dbHandle.GetWithOptions(key, interopOptions);
            }
            else
            {
                if (_readerHandle == null)
                    throw new SlateDbException("Reader handle is null");

                return await _readerHandle.GetWithOptions(key, interopOptions);
            }
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Get failed: {ex.Message}", ex);
        }
    }
}
