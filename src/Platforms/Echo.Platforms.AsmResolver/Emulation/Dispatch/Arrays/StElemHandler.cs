using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>stelem</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(
        CilCode.Stelem,
        CilCode.Stelem_I,
        CilCode.Stelem_I1,
        CilCode.Stelem_I2,
        CilCode.Stelem_I4,
        CilCode.Stelem_I8,
        CilCode.Stelem_R4,
        CilCode.Stelem_R8,
        CilCode.Stelem_Ref)]
    public class StElemHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;

            // Determine parameters.
            var elementType = GetElementType(context, instruction);
            var value = stack.Pop(elementType);
            var arrayIndex = stack.Pop();
            var arrayAddress = stack.Pop();
            var arrayLength = factory.BitVectorPool.Rent(32, false);
            
            try
            {
                // Concretize pushed address.
                long? resolvedAddress = arrayAddress.Contents.IsFullyKnown
                    ? arrayAddress.Contents.AsSpan().ReadNativeInteger(context.Machine.Is32Bit)
                    : context.Machine.UnknownResolver.ResolveDestinationPointer(context, instruction, arrayAddress);

                switch (resolvedAddress)
                {
                    case null:
                        // If address is unknown even after resolution, assume it writes to "somewhere" successfully.
                        return CilDispatchResult.Success();

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
                        
                        // If index is unknown even after resolution, assume it writes to "somewhere" successfully.
                        handle.ReadArrayLength(arrayLength);
                        if (resolvedIndex.HasValue && arrayLength.IsFullyKnown)
                        {
                            // Bounds check.
                            if (resolvedIndex >= arrayLength.AsSpan().ReadNativeInteger(context.Machine.Is32Bit))
                                return CilDispatchResult.IndexOutOfRange(context);
                            
                            //Write
                            handle.WriteArrayElement(elementType, resolvedIndex.Value, value);
                        }

                        break;
                }

                return CilDispatchResult.Success();
            }
            finally
            {
                // Return rented values.
                factory.BitVectorPool.Return(arrayLength);
                factory.BitVectorPool.Return(arrayAddress.Contents);
                factory.BitVectorPool.Return(arrayIndex.Contents);
                factory.BitVectorPool.Return(value);
            }
        }
        
        private static TypeSignature GetElementType(CilExecutionContext context, CilInstruction instruction)
        {
            var factory = context.Machine.ValueFactory.ContextModule.CorLibTypeFactory;
            return instruction.OpCode.Code switch
            {
                CilCode.Stelem => ((ITypeDefOrRef) instruction.Operand!).ToTypeSignature(),
                CilCode.Stelem_I => factory.IntPtr,
                CilCode.Stelem_I1 => factory.SByte,
                CilCode.Stelem_I2 => factory.Int16,
                CilCode.Stelem_I4 => factory.Int32,
                CilCode.Stelem_I8 => factory.Int64,
                CilCode.Stelem_R4 => factory.Single,
                CilCode.Stelem_R8 => factory.Double,
                CilCode.Stelem_Ref => factory.Object,
                _ => throw new ArgumentOutOfRangeException(nameof(instruction))
            };
        }
    }
}