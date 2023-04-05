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
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;

            // Allocate the new object.
            var instanceType = ((IMethodDescriptor) instruction.Operand!).DeclaringType!.ToTypeSignature();
            var address = factory.RentNativeInteger(context.Machine.Heap.AllocateObject(instanceType, false));
            try
            {
                // Push onto stack.
                stack.Push(address, instanceType);

                // Invoke constructor.
                var result = base.Dispatch(context, instruction);

                // If successful, push the resulting object onto the stack.
                if (result.IsSuccess)
                    stack.Push(address, instanceType);

                return result;
            }
            finally
            {
                factory.BitVectorPool.Return(address);
            }
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