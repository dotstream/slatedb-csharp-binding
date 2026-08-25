using System.Runtime.InteropServices;
using SlateDb.Converter;
using SlateDb.Options;

namespace SlateDb;

/// <summary>
/// Application-defined merge operator, installed via
/// <see cref="SlateDbBuilder{K,V}.WithMergeOperator(SlatedbMergeOperatorFn)"/>.
///
/// Given <paramref name="key"/>, the current value (if any, indicated by
/// <paramref name="existingValuePresent"/>/<paramref name="existingValue"/>), and the merge
/// operand, computes the new value and writes a pointer/length to it into
/// <paramref name="outValue"/>/<paramref name="outValueLen"/>. The returned buffer is later
/// freed via the paired <see cref="SlateDbFreeMergeResultFn"/>, if one was supplied.
/// </summary>
/// <returns><c>true</c> if the merge succeeded; <c>false</c> to fail the operation.</returns>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate bool SlatedbMergeOperatorFn(
    byte* key,
    nuint keyLen,
    [MarshalAs(UnmanagedType.I1)] bool existingValuePresent,
    byte* existingValue,
    nuint existingValueLen,
    byte* operand,
    nuint operandLen,
    byte** outValue,
    nuint* outValueLen);

/// <summary>
/// Callback that frees a result buffer previously produced by a <see cref="SlatedbMergeOperatorFn"/>.
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate void SlateDbFreeMergeResultFn(byte* ptr,
    nuint len);

// for testing
internal static class MergeOperators
{
    internal static unsafe bool MergeOperator(
        byte* key,
        nuint key_len,
        [MarshalAs(UnmanagedType.I1)] bool existing_value_present,
        byte* existing_value,
        nuint existing_value_len,
        byte* operand,
        nuint operand_len,
        byte** out_value,
        nuint* out_value_len)
    {
        var len = (int)operand_len;
        IntPtr buffer = Marshal.AllocHGlobal(len);
        Buffer.MemoryCopy(operand, (void*)buffer, len, len);

        *out_value = (byte*)buffer;
        *out_value_len = (nuint)len;

        return true;
    }
    
    internal static unsafe bool NotMergeOperator(
        byte* key,
        nuint key_len,
        [MarshalAs(UnmanagedType.I1)] bool existing_value_present,
        byte* existing_value,
        nuint existing_value_len,
        byte* operand,
        nuint operand_len,
        byte** out_value,
        nuint* out_value_len)
    {
        if (existing_value_present)
        {
            var len = (int)existing_value_len;
            IntPtr buffer = Marshal.AllocHGlobal(len);
            Buffer.MemoryCopy(existing_value, (void*)buffer, len, len);

            *out_value = (byte*)buffer;
            *out_value_len = (nuint)len;

            return true;
        }
        else
        {
            var len = (int)operand_len;
            IntPtr buffer = Marshal.AllocHGlobal(len);
            Buffer.MemoryCopy(operand, (void*)buffer, len, len);

            *out_value = (byte*)buffer;
            *out_value_len = (nuint)len;

            return true;
        }
    }
    
    internal static unsafe void FreeMergeResult(byte* ptr, nuint len)
    {
        Marshal.FreeHGlobal((IntPtr)ptr);
    }
}

public sealed partial class SlateDb<K,V>
{
    /// <summary>Appends a merge operand for <paramref name="key"/>, applied via the installed merge operator. Write-mode only.</summary>
    public void Merge(K key, V value)
        => Merge(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), null, null);

    /// <summary>Appends a merge operand for <paramref name="key"/> using custom merge and write options. Write-mode only.</summary>
    public void Merge(K key, V value, MergeOptions mergeOptions, WriteOptions writeOptions)
        => Merge(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), mergeOptions,  writeOptions);

    /// <summary>Appends a raw merge operand for <paramref name="key"/> using custom merge and write options. Write-mode only.</summary>
    public void Merge(byte[]? key, byte[]? operand, MergeOptions? mergeOptions, WriteOptions? writeOptions)
    {
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(operand);

        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        mergeOptions ??= MergeOptions.NoExpiry;
        writeOptions ??= WriteOptions.Default;

        try
        {
            if (mergeOptions.TtlType == TtlType.NoExpiry && !writeOptions.AwaitDurable)
            {
                _dbHandle.Merge(key, operand).GetAwaiter().GetResult();
            }
            else
            {
                _dbHandle.MergeWithOptions(
                    key, operand,
                    Interop.OptionsConverters.ToInterop(mergeOptions),
                    Interop.OptionsConverters.ToInterop(writeOptions)).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Merge failed: {ex.Message}", ex);
        }
    }

    /// <summary>Appends a merge operand for <paramref name="key"/> asynchronously, applied via the installed merge operator. Write-mode only.</summary>
    public Task MergeAsync(K key, V value)
        => MergeAsync(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), null, null);

    /// <summary>Appends a merge operand for <paramref name="key"/> asynchronously using custom merge and write options. Write-mode only.</summary>
    public Task MergeAsync(K key, V value, MergeOptions mergeOptions, WriteOptions writeOptions)
        => MergeAsync(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), mergeOptions, writeOptions);

    /// <summary>Appends a raw merge operand for <paramref name="key"/> asynchronously using custom merge and write options. Write-mode only.</summary>
    public async Task MergeAsync(byte[]? key, byte[]? operand, MergeOptions? mergeOptions, WriteOptions? writeOptions)
    {
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(operand);

        if (_dbHandle == null)
            throw new SlateDbException("Database handle is null");

        mergeOptions ??= MergeOptions.NoExpiry;
        writeOptions ??= WriteOptions.Default;

        try
        {
            if (mergeOptions.TtlType == TtlType.NoExpiry && !writeOptions.AwaitDurable)
            {
                await _dbHandle.Merge(key, operand);
            }
            else
            {
                await _dbHandle.MergeWithOptions(
                    key, operand,
                    Interop.OptionsConverters.ToInterop(mergeOptions),
                    Interop.OptionsConverters.ToInterop(writeOptions));
            }
        }
        catch (Exception ex) when (ex is not SlateDbException)
        {
            throw new SlateDbException($"Merge failed: {ex.Message}", ex);
        }
    }
}