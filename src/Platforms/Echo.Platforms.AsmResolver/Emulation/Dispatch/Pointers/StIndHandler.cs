using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
using Echo.Core;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Pointers
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>stind</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(
        CilCode.Stobj,
        CilCode.Stind_I,
        CilCode.Stind_I1,
        CilCode.Stind_I2,
        CilCode.Stind_I4,
        CilCode.Stind_I8,
        CilCode.Stind_R4,
        CilCode.Stind_R8,
        CilCode.Stind_Ref)]
    public class StIndHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;

            // Determine parameters.
            var elementType = GetElementType(context, instruction);
            var value = stack.Pop(elementType);
            var address = stack.Pop().Contents;
            var result = factory.BitVectorPool.Rent(
                (int) factory.GetTypeValueMemoryLayout(elementType).Size * 8, 
                false);
            
            try
            {
                // Write memory if fully known address, else leave result unknown.
                var addressSpan = address.AsSpan();
                switch (addressSpan)
                {
                    case { IsFullyKnown: false }:
                        throw new CilEmulatorException("Attempted to write to an unknown memory pointer.");
                        break;

                    case { IsZero.Value: TrileanValue.True }:
                        return CilDispatchResult.NullReference(context);

                    default:
                        context.Machine.Memory.Write(addressSpan.ReadNativeInteger(context.Machine.Is32Bit), value);
                        break;
                }

                return CilDispatchResult.Success();
            }
            finally
            {
                // Return rented values.
                factory.BitVectorPool.Return(address);
                factory.BitVectorPool.Return(value);
                factory.BitVectorPool.Return(result);
            }
        }
        
        private static TypeSignature GetElementType(CilExecutionContext context, CilInstruction instruction)
        {
            var factory = context.Machine.ValueFactory.ContextModule.CorLibTypeFactory;
            return instruction.OpCode.Code switch
            {
                CilCode.Stobj => ((ITypeDefOrRef) instruction.Operand!).ToTypeSignature(),
                CilCode.Stind_I => factory.IntPtr,
                CilCode.Stind_I1 => factory.SByte,
                CilCode.Stind_I2 => factory.Int16,
                CilCode.Stind_I4 => factory.Int32,
                CilCode.Stind_I8 => factory.Int64,
                CilCode.Stind_R4 => factory.Single,
                CilCode.Stind_R8 => factory.Double,
                CilCode.Stind_Ref => factory.Object,
                _ => throw new ArgumentOutOfRangeException(nameof(instruction))
            };
        }
    }
}