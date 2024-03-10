using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Invocation;

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
            
            // Allocate the new object.
            var constructor = (IMethodDescriptor) instruction.Operand!;
            var instanceType = constructor.DeclaringType!.ToTypeSignature();

            var arguments = GetArguments(context, constructor);
            try
            {
                var allocation = context.Machine.Allocator.Allocate(context, constructor, arguments);
                switch (allocation.ResultType)
                {
                    case AllocationResultType.Inconclusive:
                        throw new CilEmulatorException($"Allocation of object of type {instanceType} was inconclusive");

                    case AllocationResultType.Allocated:
                        // Insert the allocated "this" pointer into the arguments and call constructor.
                        arguments.Insert(0, allocation.Address!);
                        var result = HandleCall(context, instruction, arguments);
                
                        // If successful, push the resulting object onto the stack.
                        if (result.IsSuccess)
                            stack.Push(allocation.Address!, instanceType);

                        return result;

                    case AllocationResultType.FullyConstructed:
                        // Fully constructed objects do not have to be post-processed.
                        stack.Push(allocation.Address!, instanceType);
                        context.CurrentFrame.ProgramCounter += instruction.Size;
                        return CilDispatchResult.Success();

                    case AllocationResultType.Exception:
                        return CilDispatchResult.Exception(allocation.ExceptionObject);

                    default:
                        throw new ArgumentOutOfRangeException();
                }
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