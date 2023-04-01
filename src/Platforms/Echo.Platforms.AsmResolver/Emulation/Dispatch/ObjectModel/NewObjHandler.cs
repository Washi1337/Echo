using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>newobj</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Newobj)]
    public class NewObjHandler : CallHandlerBase
    {
        /// <inheritdoc />
        public override CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            // Allocate the new object.
            var instanceType = ((IMethodDescriptor) instruction.Operand!).DeclaringType!.ToTypeSignature();
            var address = context.Machine.ValueFactory.RentNativeInteger(false);
            address.AsSpan().Write(context.Machine.Heap.AllocateObject(instanceType, false));
            
            // Push onto stack.
            var stackSlot = context.CurrentFrame.EvaluationStack.Push(address, instanceType);
            
            // Invoke constructor.
            var result = base.Dispatch(context, instruction);

            // If successful, push the resulting object onto the stack.
            if (result.IsSuccess)
                context.CurrentFrame.EvaluationStack.Push(stackSlot);

            return result;
        }

        /// <inheritdoc />
        protected override MethodDevirtualizationResult DevirtualizeMethod(
            CilExecutionContext context,
            CilInstruction instruction,
            IList<BitVector> arguments)
        {
            return new MethodDevirtualizationResult((IMethodDescriptor) instruction.Operand!);
        }
    }
}