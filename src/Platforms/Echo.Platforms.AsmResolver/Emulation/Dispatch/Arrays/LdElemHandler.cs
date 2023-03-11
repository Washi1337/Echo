using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Core;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldelem</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(
        CilCode.Ldelem,
        CilCode.Ldelem_I,
        CilCode.Ldelem_I1,
        CilCode.Ldelem_I2,
        CilCode.Ldelem_I4,
        CilCode.Ldelem_I8,
        CilCode.Ldelem_R4,
        CilCode.Ldelem_R8,
        CilCode.Ldelem_Ref,
        CilCode.Ldelem_U1,
        CilCode.Ldelem_U2,
        CilCode.Ldelem_U4)]
    public class LdElemHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;

            // Determine parameters.
            var elementType = GetElementType(context, instruction);
            var arrayIndex = stack.Pop().Contents;
            var arrayAddress = stack.Pop().Contents;
            var result = factory.BitVectorPool.Rent(
                (int) factory.GetTypeValueMemoryLayout(elementType).Size * 8, 
                false);

            try
            {
                // Read element if fully known address and index, else leave result unknown.
                var arrayAddressSpan = arrayAddress.AsSpan();
                var arrayIndexSpan = arrayIndex.AsSpan();

                switch (arrayAddressSpan)
                {
                    case { IsFullyKnown: false }:
                        break;

                    case { IsZero.Value: TrileanValue.True }:
                        return CilDispatchResult.NullReference(context);

                    default:
                        var arraySpan = context.Machine.Heap.GetObjectSpan(arrayAddressSpan.ReadNativeInteger(context.Machine.Is32Bit));
                        long length = arraySpan.SliceArrayLength(factory).ReadNativeInteger(context.Machine.Is32Bit);

                        // Leave result unknown if index is not fully known.
                        if (arrayIndexSpan.IsFullyKnown)
                        {
                            int index = arrayIndexSpan.I32;
                            if (index >= length)
                                return CilDispatchResult.IndexOutOfRange(context);

                            result.AsSpan().Write(arraySpan.SliceArrayElement(factory, elementType, index));
                        }

                        break;
                }
                
                // Push.
                stack.Push(result, elementType);
                return CilDispatchResult.Success();
            }
            finally
            {
                // Return rented values.
                factory.BitVectorPool.Return(arrayIndex);
                factory.BitVectorPool.Return(arrayAddress);
                factory.BitVectorPool.Return(result);
            }
        }
        
        private static TypeSignature GetElementType(CilExecutionContext context, CilInstruction instruction)
        {
            var factory = context.Machine.ValueFactory.ContextModule.CorLibTypeFactory;
            return instruction.OpCode.Code switch
            {
                CilCode.Ldelem => ((ITypeDefOrRef) instruction.Operand!).ToTypeSignature(),
                CilCode.Ldelem_I => factory.IntPtr,
                CilCode.Ldelem_I1 => factory.SByte,
                CilCode.Ldelem_I2 => factory.Int16,
                CilCode.Ldelem_I4 => factory.Int32,
                CilCode.Ldelem_I8 => factory.Int64,
                CilCode.Ldelem_R4 => factory.Single,
                CilCode.Ldelem_R8 => factory.Double,
                CilCode.Ldelem_Ref => factory.Object,
                CilCode.Ldelem_U1 => factory.Byte,
                CilCode.Ldelem_U2 => factory.UInt16,
                CilCode.Ldelem_U4 => factory.UInt32,
                _ => throw new ArgumentOutOfRangeException(nameof(instruction))
            };
        }
    }
}