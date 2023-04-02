using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;

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
            var arrayIndex = stack.Pop();
            var arrayAddress = stack.Pop();
            var result = factory.RentValue(elementType, false);
            var arrayLength = factory.RentNativeInteger(false);
            
            try
            {
                // Concretize pushed address.
                long? resolvedAddress = arrayAddress.Contents.IsFullyKnown
                    ? arrayAddress.Contents.AsSpan().ReadNativeInteger(context.Machine.Is32Bit)
                    : context.Machine.UnknownResolver.ResolveSourcePointer(context, instruction, arrayAddress);

                switch (resolvedAddress)
                {
                    case null:
                        // If address is unknown even after resolution, assume it reads from "somewhere" successfully.
                        break;

                    case 0:
                        // A null reference was passed.
                        return CilDispatchResult.NullReference(context);

                    case { } actualAddress:
                        // A non-null reference was passed.
                        var handle = actualAddress.AsObjectHandle(context.Machine);
                        
                        // Concretize pushed index.
                        long? resolvedIndex = arrayIndex.Contents.IsFullyKnown
                            ? arrayIndex.Contents.AsSpan().ReadNativeInteger(context.Machine.Is32Bit)
                            : context.Machine.UnknownResolver.ResolveArrayIndex(context, instruction, actualAddress, arrayIndex);

                        // Leave result unknown if index is not fully known.
                        handle.ReadArrayLength(arrayLength);
                        if (resolvedIndex.HasValue && arrayLength.IsFullyKnown)
                        {
                            // Bounds check.
                            if (resolvedIndex >= arrayLength.AsSpan().ReadNativeInteger(context.Machine.Is32Bit))
                                return CilDispatchResult.IndexOutOfRange(context);

                            handle.ReadArrayElement(elementType, resolvedIndex.Value, result);
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
                factory.BitVectorPool.Return(arrayLength);
                factory.BitVectorPool.Return(arrayIndex.Contents);
                factory.BitVectorPool.Return(arrayAddress.Contents);
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