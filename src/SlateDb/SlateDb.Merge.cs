using System.Runtime.InteropServices;
using SlateDb.Converter;
using SlateDb.Handle;
using SlateDb.Interop;
using SlateDb.Options;

namespace SlateDb;

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
    public void Merge(K key, V value) 
        => Merge(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), null, null);
    
    public void Merge(K key, V value, MergeOptions mergeOptions, WriteOptions writeOptions)
        => Merge(_keyConverter.ConvertClassToBytes(key), _valueConverter.ConvertClassToBytes(value), mergeOptions,  writeOptions);

    public void Merge(byte[]? key, byte[]? value, MergeOptions? mergeOptions, WriteOptions? writeOptions)
    {
        CheckSlateDbMode(true);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(_handle == null, this);
        
        unsafe
        {
            fixed (byte* keyPtr = key)
            fixed (byte* valuePtr = value)
            {
                mergeOptions ??= MergeOptions.NoExpiry;
                writeOptions ??= WriteOptions.Default;

                var nativePut = new slatedb_merge_options_t {
                    ttl_type = (byte)mergeOptions.TtlType,
                    ttl_value = (ulong)mergeOptions.TtlValue.TotalMilliseconds
                };
                
                var nativeWrite = new slatedb_write_options_t {
                    await_durable = writeOptions.AwaitDurable
                };
                
                NativeMethods.slatedb_db_merge_with_options(
                    _handle.GetCSdbHandle<slatedb_db_t>(),
                    keyPtr,
                    key != null ? (nuint)key.Length : 0,
                    valuePtr,
                    value != null ? (nuint)value.Length : 0,
                    &nativePut, &nativeWrite, 
                    null).ThrowOnError();
            }
        }
    }
}