using SlateDb.Converter;
using SlateDb.Options;

namespace SlateDb;

public sealed partial class SlateDb<K, V>
{
    /// <summary>
    /// A mutable batch of write operations, created via <see cref="NewWriteBatch"/> and applied
    /// atomically via <see cref="Write(SlateDbWriteBatch)"/> or <see cref="WriteAsync(SlateDbWriteBatch)"/>.
    ///
    /// A batch is single-use: once submitted, it should not be reused.
    /// </summary>
    public class SlateDbWriteBatch : IDisposable
    {
        private readonly ISlateDbConverter<K>? _keyConverter;
        private readonly ISlateDbConverter<V>? _valueConverter;
        private readonly Interop.WriteBatch _batch;
        private bool _disposed;

        internal Interop.WriteBatch NativeHandle => _batch;

        internal SlateDbWriteBatch(ISlateDbConverter<K>? keyConverter, ISlateDbConverter<V>? valueConverter)
        {
            _keyConverter = keyConverter;
            _valueConverter = valueConverter;
            _batch = new Interop.WriteBatch();
        }

        /// <summary>Appends a put operation to the batch.</summary>
        public void Put(K key, V value) =>
            Put(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), null);

        /// <summary>Appends a put operation to the batch, using custom put options.</summary>
        public void Put(K key, V value, PutOptions options) =>
            Put(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), options);

        /// <summary>Appends a raw put operation to the batch, using custom put options.</summary>
        public void Put(byte[]? key, byte[]? value, PutOptions? options)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);

            options ??= PutOptions.NoExpiry;

            try
            {
                if (options.TtlType == TtlType.NoExpiry)
                {
                    _batch.Put(key, value);
                }
                else
                {
                    _batch.PutWithOptions(key, value, Interop.OptionsConverters.ToInterop(options));
                }
            }
            catch (Exception ex) when (ex is not SlateDbException)
            {
                throw new SlateDbException($"WriteBatch.Put failed: {ex.Message}", ex);
            }
        }

        /// <summary>Appends a raw delete operation to the batch.</summary>
        public void Delete(byte[]? key)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(key);

            try
            {
                _batch.Delete(key);
            }
            catch (Exception ex) when (ex is not SlateDbException)
            {
                throw new SlateDbException($"WriteBatch.Delete failed: {ex.Message}", ex);
            }
        }

        /// <summary>Appends a delete operation to the batch.</summary>
        public void Delete(K key) => Delete(_keyConverter.ConvertClassToBytes(key));

        /// <summary>Appends a merge operation to the batch.</summary>
        public void Merge(K key, V operand) =>
            Merge(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(operand), null);

        /// <summary>Appends a merge operation to the batch, using custom merge options.</summary>
        public void Merge(K key, V operand, MergeOptions options) =>
            Merge(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(operand), options);

        /// <summary>Appends a raw merge operation to the batch, using custom merge options.</summary>
        public void Merge(byte[]? key, byte[]? operand, MergeOptions? options)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(operand);

            options ??= MergeOptions.NoExpiry;

            try
            {
                if (options.TtlType == TtlType.NoExpiry)
                {
                    _batch.Merge(key, operand);
                }
                else
                {
                    _batch.MergeWithOptions(key, operand, Interop.OptionsConverters.ToInterop(options));
                }
            }
            catch (Exception ex) when (ex is not SlateDbException)
            {
                throw new SlateDbException($"WriteBatch.Merge failed: {ex.Message}", ex);
            }
        }

        /// <summary>Releases the underlying native batch handle.</summary>
        public void Dispose()
        {
            if (_disposed) return;
            _batch?.Dispose();
            _disposed = true;
        }
    }

    /// <summary>Creates a new, empty <see cref="SlateDbWriteBatch"/>. Write-mode only.</summary>
    public SlateDbWriteBatch NewWriteBatch()
    {
        CheckSlateDbMode(true);
        return new SlateDbWriteBatch(_keyConverter, _valueConverter);
    }

    /// <summary>Applies all operations in <paramref name="batch"/> atomically. Write-mode only.</summary>
    public void Write(SlateDbWriteBatch batch)
        => Write(batch, null);

    /// <summary>Applies all operations in <paramref name="batch"/> atomically, using custom write options. Write-mode only.</summary>
    public void Write(SlateDbWriteBatch batch, WriteOptions? options)
    {
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        options ??= WriteOptions.Default;

        try
        {
            var handle = options.AwaitDurable
                ? _dbHandle.WriteWithOptions(batch.NativeHandle, Interop.OptionsConverters.ToInterop(options)).GetAwaiter().GetResult()
                : _dbHandle.Write(batch.NativeHandle).GetAwaiter().GetResult();

            Interop.UniffiHelpers.HandleWriteResult(handle, options.AwaitDurable);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Write batch failed: {ex.Message}", ex);
        }
    }

    /// <summary>Applies all operations in <paramref name="batch"/> atomically, asynchronously. Write-mode only.</summary>
    public Task WriteAsync(SlateDbWriteBatch batch)
        => WriteAsync(batch, null);

    /// <summary>Applies all operations in <paramref name="batch"/> atomically and asynchronously, using custom write options. Write-mode only.</summary>
    public async Task WriteAsync(SlateDbWriteBatch batch, WriteOptions? options)
    {
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        options ??= WriteOptions.Default;

        try
        {
            var handle = options.AwaitDurable
                ? await _dbHandle.WriteWithOptions(batch.NativeHandle, Interop.OptionsConverters.ToInterop(options))
                : await _dbHandle.Write(batch.NativeHandle);

            await Interop.UniffiHelpers.HandleWriteResultAsync(handle, options.AwaitDurable);
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Write batch failed: {ex.Message}", ex);
        }
    }
}
