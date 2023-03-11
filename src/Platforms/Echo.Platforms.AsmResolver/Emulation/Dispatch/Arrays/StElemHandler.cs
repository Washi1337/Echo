using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Core;
using Echo.Concrete;

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
            var arrayIndex = stack.Pop().Contents;
            var arrayAddress = stack.Pop().Contents;
            
            try
            {
                // Write memory if fully known address, else leave result unknown.
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

                        if (!arrayIndexSpan.IsFullyKnown)
                        {
                            throw new CilEmulatorException("Attempted to write to an unknown array index.");
                        }
                        else
                        {
                            int index = arrayIndexSpan.I32;
                            if (index >= length)
                                return CilDispatchResult.IndexOutOfRange(context);

                            arraySpan.SliceArrayElement(factory, elementType, index).Write(value);
                        }
                        
                        break;
                }

                return CilDispatchResult.Success();
            }
            finally
            {
                // Return rented values.
                factory.BitVectorPool.Return(arrayAddress);
                factory.BitVectorPool.Return(arrayIndex);
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