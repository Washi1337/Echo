using System;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using static AsmResolver.PE.DotNet.Cil.CilCode;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Misc
{
    /// <summary>
    /// Implements a CIL instruction handler for conversion operations.
    /// </summary>
    [DispatcherTableEntry(
        Conv_I, Conv_I1, Conv_I2, Conv_I4, Conv_I8,
        Conv_U, Conv_U1, Conv_U2, Conv_U4, Conv_U8,
        Conv_R4, Conv_R8, Conv_R_Un, 
        Conv_Ovf_I, Conv_Ovf_I1, Conv_Ovf_I2, Conv_Ovf_I4, Conv_Ovf_I8,
        Conv_Ovf_U, Conv_Ovf_U1, Conv_Ovf_U2, Conv_Ovf_U4, Conv_Ovf_U8,
        Conv_Ovf_I_Un, Conv_Ovf_I1_Un, Conv_Ovf_I2_Un, Conv_Ovf_I4_Un, Conv_Ovf_I8_Un,
        Conv_Ovf_U_Un, Conv_Ovf_U1_Un, Conv_Ovf_U2_Un, Conv_Ovf_U4_Un, Conv_Ovf_U8_Un
        )]
    public class ConvHandler : FallThroughOpCodeHandler
    {
        private static readonly BitVector Int8Min = (long) sbyte.MinValue;
        private static readonly BitVector Int8Max = (long) sbyte.MaxValue;
        private static readonly BitVector Int16Min = (long) short.MinValue;
        private static readonly BitVector Int16Max = (long) short.MaxValue; 
        private static readonly BitVector Int32Min = (long) int.MinValue;
        private static readonly BitVector Int32Max = (long) int.MaxValue; 
        private static readonly BitVector Int64Min = long.MinValue;
        private static readonly BitVector Int64Max = long.MaxValue; 
        private static readonly BitVector UInt8Min = (long) byte.MinValue;
        private static readonly BitVector UInt8Max = (long) byte.MaxValue;
        private static readonly BitVector UInt16Min = (long) ushort.MinValue;
        private static readonly BitVector UInt16Max = (long) ushort.MaxValue; 
        private static readonly BitVector UInt32Min = (long) uint.MinValue;
        private static readonly BitVector UInt32Max = (long) uint.MaxValue;
        private static readonly BitVector UInt64Min = unchecked((long) ulong.MinValue);
        private static readonly BitVector UInt64Max = unchecked((long) ulong.MaxValue);

        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;

            var targetType = GetTargetType(context, instruction.OpCode.Code);
            bool isSigned = IsSigned(instruction.OpCode.Code);
            var value = stack.Pop();

            try
            {
                var (result, overflow) = value.TypeHint switch
                {
                    StackSlotTypeHint.Integer => HandleIntegerConversion(context, value.Contents, isSigned, targetType),
                    StackSlotTypeHint.Float => HandleFloatConversion(context, value.Contents, targetType),
                    _ => throw new ArgumentOutOfRangeException(nameof(value.TypeHint))
                };

                if (HasOverflowCheck(instruction.OpCode.Code) && overflow)
                    return CilDispatchResult.Overflow(context);

                stack.Push(result);
                return CilDispatchResult.Success();
            }
            finally
            {
                factory.BitVectorPool.Return(value.Contents);
            }
        }

        private static (StackSlot Result, Trilean Overflow) HandleIntegerConversion(
            CilExecutionContext context,
            BitVector value, 
            bool isSigned,
            TypeSignature targetType)
        {
            BitVector? result = null;
            
            var factory = context.Machine.ValueFactory;
            var pool = factory.BitVectorPool;
            
            // To make comparisons (for overflow checks), we need to ensure both integers are the same size.
            var rented = value.Resize(64, isSigned);

            try
            {
                Trilean overflow;
                
                var span = rented.AsSpan();
                switch (targetType.ElementType)
                {
                    case ElementType.I1:
                        overflow = span.IntegerIsLessThan(Int8Min, true) | span.IntegerIsGreaterThan(Int8Max, true);
                        result = value.Resize(8, true, pool);
                        break;

                    case ElementType.U1:
                        overflow = span.IntegerIsLessThan(UInt8Min, false) | span.IntegerIsGreaterThan(UInt8Max, false);
                        result = value.Resize(8, false, pool);
                        break;

                    case ElementType.I2:
                        overflow = span.IntegerIsLessThan(Int16Min, true) | span.IntegerIsGreaterThan(Int16Max, true);
                        result = value.Resize(16, true, pool);
                        break;

                    case ElementType.U2:
                        overflow = span.IntegerIsLessThan(UInt16Min, false) | span.IntegerIsGreaterThan(UInt16Max, false);
                        result = value.Resize(16, false, pool);
                        break;

                    case ElementType.I when context.Machine.Is32Bit:
                    case ElementType.I4:
                        overflow = span.IntegerIsLessThan(Int32Min, true) | span.IntegerIsGreaterThan(Int32Max, true);
                        result = value.Resize(32, true, pool);
                        break;

                    case ElementType.U when context.Machine.Is32Bit:
                    case ElementType.U4:
                        overflow = span.IntegerIsLessThan(UInt32Min, false) | span.IntegerIsGreaterThan(UInt32Max, false);
                        result = value.Resize(32, false, pool);
                        break;

                    case ElementType.I when !context.Machine.Is32Bit:
                    case ElementType.I8:
                        overflow = span.IntegerIsLessThan(Int64Min, true) | span.IntegerIsGreaterThan(Int64Max, true);
                        result = value.Resize(64, true, pool);
                        break;

                    case ElementType.U when !context.Machine.Is32Bit:
                    case ElementType.U8:
                        overflow = span.IntegerIsLessThan(UInt64Min, false) | span.IntegerIsGreaterThan(UInt64Max, false);
                        result = value.Resize(64, false, pool);
                        break;

                    case ElementType.R4:
                        overflow = false;
                        result = pool.Rent(32, false);
                        if (span.IsFullyKnown)
                            result.AsSpan().Write(span.I32);
                        break;

                    case ElementType.R8:
                        overflow = false;
                        result = pool.Rent(64, false);
                        if (span.IsFullyKnown)
                            result.AsSpan().Write(span.I64);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return (factory.Marshaller.ToCliValue(result, targetType), overflow);
            }
            finally
            {
                pool.Return(rented);
                pool.Return(result);
            }
        }
        
        private static (StackSlot Result, Trilean Overflow) HandleFloatConversion(
            CilExecutionContext context, 
            BitVector value,
            TypeSignature targetType)
        {
            var span = value.AsSpan();

            return span.IsFullyKnown
                ? HandleKnownFloatConversion(context, targetType, span)
                : HandleUnknownFloatConversion(context, targetType, span);
        }
        
        private static (StackSlot Result, Trilean Overflow) HandleUnknownFloatConversion(
            CilExecutionContext context,
            TypeSignature targetType, 
            Memory.BitVectorSpan span)
        {
            var pool = context.Machine.ValueFactory.BitVectorPool;
            
            StackSlot result;
            
            switch (targetType.ElementType)
            {
                case ElementType.I1:
                case ElementType.U1:
                case ElementType.I2:
                case ElementType.U2:
                case ElementType.R4:
                case ElementType.I4:
                case ElementType.U4:
                case ElementType.I when context.Machine.Is32Bit:
                case ElementType.U when context.Machine.Is32Bit:
                    result = new StackSlot(pool.Rent(32, false), StackSlotTypeHint.Integer);
                    break;

                case ElementType.I8:
                case ElementType.U8:
                case ElementType.R8:
                case ElementType.I when !context.Machine.Is32Bit:
                case ElementType.U when !context.Machine.Is32Bit:
                    result = new StackSlot(pool.Rent(64, false), StackSlotTypeHint.Integer);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return (result, Trilean.False);
        }
        
        private static (StackSlot Result, Trilean Overflow) HandleKnownFloatConversion(
            CilExecutionContext context,
            TypeSignature targetType, 
            Memory.BitVectorSpan span)
        {
            var factory = context.Machine.ValueFactory;
            var pool = factory.BitVectorPool;

            BitVector result;
            Trilean overflow;
            
            double rawValue = span.Count == 32 ? span.F32 : span.F64;
            switch (targetType.ElementType)
            {
                case ElementType.I1:
                    overflow = rawValue is < sbyte.MinValue or > sbyte.MaxValue;
                    result = pool.Rent(8, true);
                    result.AsSpan().Write((sbyte) rawValue);
                    break;

                case ElementType.U1:
                    overflow = rawValue is < byte.MinValue or > byte.MaxValue;
                    result = pool.Rent(8, true);
                    result.AsSpan().Write((byte) rawValue);
                    break;

                case ElementType.I2:
                    overflow = rawValue is < short.MinValue or > short.MaxValue;
                    result = pool.Rent(16, true);
                    result.AsSpan().Write((short) rawValue);
                    break;

                case ElementType.U2:
                    overflow = rawValue is < ushort.MinValue or > ushort.MaxValue;
                    result = pool.Rent(16, true);
                    result.AsSpan().Write((ushort) rawValue);
                    break;

                case ElementType.I when context.Machine.Is32Bit:
                case ElementType.I4:
                    overflow = rawValue is < int.MinValue or > int.MaxValue;
                    result = pool.Rent(32, true);
                    result.AsSpan().Write((int) rawValue);
                    break;

                case ElementType.U when context.Machine.Is32Bit:
                case ElementType.U4:
                    overflow = rawValue is < uint.MinValue or > uint.MaxValue;
                    result = pool.Rent(32, true);
                    result.AsSpan().Write((uint) rawValue);
                    break;

                case ElementType.I when !context.Machine.Is32Bit:
                case ElementType.I8:
                    overflow = rawValue is < long.MinValue or > long.MaxValue;
                    result = pool.Rent(64, true);
                    result.AsSpan().Write((long) rawValue);
                    break;

                case ElementType.U when !context.Machine.Is32Bit:
                case ElementType.U8:
                    overflow = rawValue is < ulong.MinValue or > ulong.MaxValue;
                    result = pool.Rent(64, true);
                    result.AsSpan().Write((ulong) rawValue);
                    break;

                case ElementType.R4:
                    overflow = false;
                    result = pool.Rent(32, true);
                    result.AsSpan().Write((float) rawValue);
                    break;

                case ElementType.R8:
                    overflow = false;
                    result = pool.Rent(64, true);
                    result.AsSpan().Write(rawValue);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            try
            {
                var stackSlot = factory.Marshaller.ToCliValue(result, targetType);
                return (stackSlot, overflow);
            }
            finally
            {
                pool.Return(result);
            }
        }

        private static TypeSignature GetTargetType(CilExecutionContext context, CilCode code)
        {
            var factory = context.Machine.ContextModule.CorLibTypeFactory;
            
            return code switch
            {
                Conv_I or Conv_Ovf_I or Conv_Ovf_I_Un => factory.IntPtr,
                Conv_I1 or Conv_Ovf_I1 or Conv_Ovf_I1_Un => factory.SByte,
                Conv_I2 or Conv_Ovf_I2 or Conv_Ovf_I2_Un => factory.Int16,
                Conv_I4 or Conv_Ovf_I4 or Conv_Ovf_I4_Un => factory.Int32,
                Conv_I8 or Conv_Ovf_I8 or Conv_Ovf_I8_Un => factory.Int64,
                Conv_U or Conv_Ovf_U or Conv_Ovf_U_Un => factory.UIntPtr,
                Conv_U1 or Conv_Ovf_U1 or Conv_Ovf_U1_Un => factory.Byte,
                Conv_U2 or Conv_Ovf_U2 or Conv_Ovf_U2_Un => factory.UInt16,
                Conv_U4 or Conv_Ovf_U4 or Conv_Ovf_U4_Un => factory.UInt32,
                Conv_U8 or Conv_Ovf_U8 or Conv_Ovf_U8_Un => factory.UInt64,
                Conv_R4 => factory.Single,
                Conv_R8 or Conv_R_Un => factory.Double,
                _ => throw new ArgumentOutOfRangeException(nameof(code))
            };
        }

        private static bool HasOverflowCheck(CilCode code)
        {
            switch (code)
            {
                case Conv_Ovf_I:
                case Conv_Ovf_I1:
                case Conv_Ovf_I2:
                case Conv_Ovf_I4:
                case Conv_Ovf_I8:
                case Conv_Ovf_U:
                case Conv_Ovf_U1:
                case Conv_Ovf_U2:
                case Conv_Ovf_U4:
                case Conv_Ovf_U8:
                case Conv_Ovf_I_Un:
                case Conv_Ovf_I1_Un:
                case Conv_Ovf_I2_Un:
                case Conv_Ovf_I4_Un:
                case Conv_Ovf_I8_Un:
                case Conv_Ovf_U_Un:
                case Conv_Ovf_U1_Un:
                case Conv_Ovf_U2_Un:
                case Conv_Ovf_U4_Un:
                case Conv_Ovf_U8_Un:
                    return true;

                default:
                    return false;
            }
        }

        private static bool IsSigned(CilCode code)
        {
            switch (code)
            {
                case Conv_I:
                case Conv_I1:
                case Conv_I2:
                case Conv_I4:
                case Conv_I8:
                case Conv_U:
                case Conv_U1:
                case Conv_U2:
                case Conv_U4:
                case Conv_U8:
                case Conv_R4:
                case Conv_R8:
                case Conv_Ovf_I:
                case Conv_Ovf_I1:
                case Conv_Ovf_I2:
                case Conv_Ovf_I4:
                case Conv_Ovf_I8:
                case Conv_Ovf_U:
                case Conv_Ovf_U1:
                case Conv_Ovf_U2:
                case Conv_Ovf_U4:
                case Conv_Ovf_U8:
                    return true;

                default:
                    return false;
            }
        }
    }
}