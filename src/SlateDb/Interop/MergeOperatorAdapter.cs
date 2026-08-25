using System.Runtime.InteropServices;

namespace SlateDb.Interop;

internal sealed unsafe class MergeOperatorAdapter : MergeOperator
{
    private readonly SlatedbMergeOperatorFn _mergeOperator;
    private readonly SlateDbFreeMergeResultFn? _freeMergeResultFn;

    internal MergeOperatorAdapter(SlatedbMergeOperatorFn mergeOperator, SlateDbFreeMergeResultFn? freeMergeResultFn)
    {
        _mergeOperator = mergeOperator;
        _freeMergeResultFn = freeMergeResultFn;
    }

    public byte[] Merge(byte[] key, byte[]? existingValue, byte[] operand)
    {
        fixed (byte* keyPtr = key)
        fixed (byte* existingPtr = existingValue)
        fixed (byte* operandPtr = operand)
        {
            byte* outValue = null;
            nuint outLen = 0;

            var ok = _mergeOperator(
                keyPtr, (nuint)key.Length,
                existingValue != null,
                existingPtr, (nuint)(existingValue?.Length ?? 0),
                operandPtr, (nuint)operand.Length,
                &outValue, &outLen);

            if (!ok)
                throw new MergeOperatorCallbackException.Failed("Merge operator declined to produce a value");

            var result = outLen > 0 ? new byte[(int)outLen] : [];
            if (outLen > 0)
                Marshal.Copy((IntPtr)outValue, result, 0, (int)outLen);

            if (outValue != null)
            {
                if (_freeMergeResultFn != null)
                    _freeMergeResultFn(outValue, outLen);
                else
                    Marshal.FreeHGlobal((IntPtr)outValue);
            }

            return result;
        }
    }
}
