using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;

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
            var genericContext = GenericContext.FromMethod(context.CurrentFrame.Method);

            // Determine parameters.
            var elementType = GetElementType(context, instruction).InstantiateGenericTypes(genericContext);
            var value = stack.Pop(elementType);
            var address = stack.Pop();
            var result = factory.RentValue(elementType, false);
            
            try
            {
                // Concretize pushed address.
                long? resolvedAddress = address.Contents.IsFullyKnown
                    ? address.Contents.AsSpan().ReadNativeInteger(context.Machine.Is32Bit)
                    : context.Machine.UnknownResolver.ResolveDestinationPointer(context, instruction, address);

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
                        context.Machine.Memory.Write(actualAddress, value);
                        break;
                }

                return CilDispatchResult.Success();
            }
            finally
            {
                // Return rented values.
                factory.BitVectorPool.Return(address.Contents);
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