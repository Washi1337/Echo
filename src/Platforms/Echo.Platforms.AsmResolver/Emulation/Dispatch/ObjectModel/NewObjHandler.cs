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
            var constructor = (IMethodDescriptor) instruction.Operand!;
            var instanceType = constructor.DeclaringType!.ToTypeSignature();
            var address = factory.CreateNativeInteger(context.Machine.Heap.AllocateObject(instanceType, true));

            var arguments = GetArguments(context, constructor);
            try
            {
                // Insert the allocated "this" pointer into the arguments.
                arguments.Insert(0, address);
                
                // Call the constructor.
                var result = HandleCall(context, instruction, arguments);
                
                // If successful, push the resulting object onto the stack.
                if (result.IsSuccess)
                    stack.Push(address, instanceType);

                return result;
            }
            finally
            {
                for (int i = 0; i < arguments.Count; i++)
                    context.Machine.ValueFactory.BitVectorPool.Return(arguments[i]);
            }
        }

        /// <inheritdoc />
        protected override bool ShouldPopInstanceObject(IMethodDescriptor method) => false;

        /// <inheritdoc />
        protected override MethodDevirtualizationResult DevirtualizeMethodInternal(
            CilExecutionContext context,
            CilInstruction instruction,
            IList<BitVector> arguments)
        {
            return MethodDevirtualizationResult.Success((IMethodDescriptor) instruction.Operand!);
        }
    }
}