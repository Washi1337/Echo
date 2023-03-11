using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Core;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Pointers
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldind</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(
        CilCode.Ldobj,
        CilCode.Ldind_I,
        CilCode.Ldind_I1,
        CilCode.Ldind_I2,
        CilCode.Ldind_I4,
        CilCode.Ldind_I8,
        CilCode.Ldind_R4,
        CilCode.Ldind_R8,
        CilCode.Ldind_Ref,
        CilCode.Ldind_U1,
        CilCode.Ldind_U2,
        CilCode.Ldind_U4)]
    public class LdIndHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;

            // Determine parameters.
            var elementType = GetElementType(context, instruction);
            var address = stack.Pop().Contents;
            var result = factory.BitVectorPool.Rent(
                (int) factory.GetTypeValueMemoryLayout(elementType).Size * 8, 
                false);

            try
            {
                // Read memory if fully known address, else leave result unknown.
                var addressSpan = address.AsSpan();
                switch (addressSpan)
                {
                    case { IsFullyKnown: false }:
                        break;

                    case { IsZero.Value: TrileanValue.True }:
                        return CilDispatchResult.NullReference(context);

                    default:
                        context.Machine.Memory.Read(addressSpan.ReadNativeInteger(context.Machine.Is32Bit), result);
                        break;
                }

                // Push.
                stack.Push(result, elementType);
                return CilDispatchResult.Success();
            }
            finally
            {
                // Return rented values.
                factory.BitVectorPool.Return(address);
                factory.BitVectorPool.Return(result);
            }
        }

        private static TypeSignature GetElementType(CilExecutionContext context, CilInstruction instruction)
        {
            var factory = context.Machine.ValueFactory.ContextModule.CorLibTypeFactory;
            return instruction.OpCode.Code switch
            {
                CilCode.Ldobj => ((ITypeDefOrRef) instruction.Operand!).ToTypeSignature(),
                CilCode.Ldind_I => factory.IntPtr,
                CilCode.Ldind_I1 => factory.SByte,
                CilCode.Ldind_I2 => factory.Int16,
                CilCode.Ldind_I4 => factory.Int32,
                CilCode.Ldind_I8 => factory.Int64,
                CilCode.Ldind_R4 => factory.Single,
                CilCode.Ldind_R8 => factory.Double,
                CilCode.Ldind_Ref => factory.Object,
                CilCode.Ldind_U1 => factory.Byte,
                CilCode.Ldind_U2 => factory.UInt16,
                CilCode.Ldind_U4 => factory.UInt32,
                _ => throw new ArgumentOutOfRangeException(nameof(instruction))
            };
        }
    }
}