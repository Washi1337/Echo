using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation;

/// <summary>
/// Provides a shim allocator that handles System.String constructors. 
/// </summary>
public class StringAllocator : IObjectAllocator
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="StringAllocator"/> class.
    /// </summary>
    public static StringAllocator Instance
    {
        get;
    } = new();

    /// <inheritdoc />
    public AllocationResult Allocate(CilExecutionContext context, IMethodDescriptor ctor, IList<BitVector> arguments)
    {
        if ((!ctor.DeclaringType?.IsTypeOf("System", "String") ?? true) || ctor.Signature is null)
            return AllocationResult.Inconclusive();

        // TODO: We may want to make this configurable. 
        foreach (var argument in arguments)
        {
            if (!argument.IsFullyKnown)
                return AllocationResult.Inconclusive();
        }

        // TODO: Add all string constructors.
        var types = ctor.Signature.ParameterTypes;
        switch (types.Count)
        {
            case 1:
                return types[0] switch
                {
                    // .ctor(char[])
                    SzArrayTypeSignature { BaseType.ElementType: ElementType.Char }
                        => ConstructCharArrayString(context, arguments),

                    // .ctor(char*)
                    PointerTypeSignature { BaseType.ElementType: ElementType.Char }
                        => ConstructCharPointerString(context, arguments),

                    // .ctor(sbyte*)
                    PointerTypeSignature { BaseType.ElementType: ElementType.I1 } 
                        => ConstructSBytePointerString(context, arguments),

                    // other
                    _ => AllocationResult.Inconclusive()
                };

            case 2:
                if (types[0].ElementType == ElementType.Char && types[1].ElementType == ElementType.I4)
                {
                    // .ctor(char, int32)
                    return ConstructRepeatedCharString(context, arguments);
                }

                // other
                return AllocationResult.Inconclusive();

            case 3:
                return types[0] switch
                {
                    // .ctor(char[], int32, int32)
                    SzArrayTypeSignature { BaseType.ElementType: ElementType.Char } 
                        => ConstructSizedCharArrayString(context, arguments),

                    // // .ctor(char*, int32, int32)
                    PointerTypeSignature { BaseType.ElementType: ElementType.Char }
                        => ConstructSizedCharPointerString(context, arguments),

                    // // .ctor(sbyte*, int32, int32)
                    PointerTypeSignature { BaseType.ElementType: ElementType.I1 } 
                        => ConstructSizedSBytePointerString(context, arguments),

                    // other
                    _ => AllocationResult.Inconclusive()
                };
        }

        return AllocationResult.Inconclusive();
    }

    private static AllocationResult ConstructRepeatedCharString(CilExecutionContext context, IList<BitVector> arguments)
    {
        char c = (char)arguments[0].AsSpan().U16;
        int length = arguments[1].AsSpan().I32;

        long result = context.Machine.Heap.AllocateString(new string(c, length));

        return AllocationResult.FullyConstructed(context.Machine.ValueFactory.RentNativeInteger(result));
    }

    private static AllocationResult ConstructCharArrayString(CilExecutionContext context, IList<BitVector> arguments)
    {
        // Get array behind object.
        var array = arguments[0].AsObjectHandle(context.Machine);

        // Read chars from array.
        long result = array.Address != 0
            ? context.Machine.Heap.AllocateString(array.ReadArrayData())
            : 0;

        return AllocationResult.FullyConstructed(context.Machine.ValueFactory.RentNativeInteger(result));
    }
    
    private static AllocationResult ConstructSizedCharArrayString(CilExecutionContext context,
        IList<BitVector> arguments)
    {
        // Get array behind object.
        var array = arguments[0].AsObjectHandle(context.Machine);
        int startIndex = arguments[1].AsSpan().I32;
        int length = arguments[2].AsSpan().I32;

        // Read chars from array.
        long result = array.Address != 0
            ? context.Machine.Heap.AllocateString(array.ReadArrayData(startIndex, length))
            : 0;

        return AllocationResult.FullyConstructed(context.Machine.ValueFactory.RentNativeInteger(result));
    }

    private static AllocationResult ConstructCharPointerString(CilExecutionContext context, IList<BitVector> arguments)
    {
        // Measure bounds.
        long startAddress = arguments[0].AsSpan().ReadNativeInteger(context.Machine.Is32Bit);
        long length = GetNullTerminatedStringLength(context, startAddress, sizeof(char));

        // Construct string.
        return ConstructSizedCharPointerString(context, startAddress, 0, (int)length);
    }

    private static AllocationResult ConstructSizedCharPointerString(CilExecutionContext context,
        IList<BitVector> arguments)
    {
        // Measure bounds.
        long startAddress = arguments[0].AsSpan().ReadNativeInteger(context.Machine.Is32Bit);
        int startIndex = arguments[1].AsSpan().I32;
        int length = arguments[2].AsSpan().I32;

        // Construct string.
        return ConstructSizedCharPointerString(context, startAddress, startIndex, length);
    }

    private static AllocationResult ConstructSizedCharPointerString(
        CilExecutionContext context,
        long startAddress,
        int startIndex,
        int length)
    {
        // Read Unicode bytes.
        var totalData = new BitVector(length * sizeof(char) * 8, false);
        context.Machine.Memory.Read(startAddress + startIndex * sizeof(char), totalData);

        // Construct string.
        long result = context.Machine.Heap.AllocateString(totalData);
        return AllocationResult.FullyConstructed(context.Machine.ValueFactory.RentNativeInteger(result));
    }

    private static AllocationResult ConstructSBytePointerString(CilExecutionContext context, IList<BitVector> arguments)
    {
        // Measure bounds.
        long startAddress = arguments[0].AsSpan().ReadNativeInteger(context.Machine.Is32Bit);
        long length = GetNullTerminatedStringLength(context, startAddress, sizeof(sbyte));

        // Construct string.
        return ConstructSizedSBytePointerString(context, startAddress, (int)length);
    }

    private static AllocationResult ConstructSizedSBytePointerString(CilExecutionContext context,
        IList<BitVector> arguments)
    {
        // Measure bounds.
        long startAddress = arguments[0].AsSpan().ReadNativeInteger(context.Machine.Is32Bit);
        int startIndex = arguments[1].AsSpan().I32;
        int length = arguments[2].AsSpan().I32;

        // Construct string.
        return ConstructSizedSBytePointerString(context, startAddress + startIndex, length);
    }

    private static AllocationResult ConstructSizedSBytePointerString(CilExecutionContext context, long startAddress,
        int length)
    {
        // Read ASCII bytes.
        var totalData = new BitVector(length * 8, false);
        context.Machine.Memory.Read(startAddress, totalData);

        // Convert all ASCII bytes to Unicode bytes.
        var widened = new BitVector(totalData.Count * 2, false);
        for (int i = 0; i < totalData.ByteCount; i++)
        {
            widened.Bits[i * 2] = totalData.Bits[i];
            widened.KnownMask[i * 2] = totalData.KnownMask[i];
            widened.KnownMask[i * 2 + 1] = 0xFF;
        }

        // Construct string.
        long result = context.Machine.Heap.AllocateString(widened);
        return AllocationResult.FullyConstructed(context.Machine.ValueFactory.RentNativeInteger(result));
    }

    private static long GetNullTerminatedStringLength(CilExecutionContext context, long startAddress, int charSize)
    {
        long endAddress = startAddress;

        var singleChar = context.Machine.ValueFactory.BitVectorPool.Rent(charSize * 8, false);
        try
        {
            bool foundNullTerminator = false;
            while (!foundNullTerminator)
            {
                context.Machine.Memory.Read(endAddress, singleChar);
                switch (singleChar.AsSpan().IsZero.Value)
                {
                    case TrileanValue.True:
                        // We definitely found a zero.
                        foundNullTerminator = true;
                        break;

                    case TrileanValue.False:
                        // We definitely found a non-zero value.
                        endAddress += charSize;
                        break;

                    case TrileanValue.Unknown:
                        // We are not sure this is a zero. We cannot continue.
                        throw new CilEmulatorException(
                            $"Attempted to read a null-terminated string at 0x{startAddress:X8} where the final size is uncertain.");

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        finally
        {
            context.Machine.ValueFactory.BitVectorPool.Return(singleChar);
        }

        return (endAddress - startAddress) / charSize;
    }
}