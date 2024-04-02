using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow;
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
            var constructor = (IMethodDescriptor)instruction.Operand!;
            var instanceType = constructor.DeclaringType!.ToTypeSignature();

            var arguments = GetArguments(context, constructor);
            try
            {
                return instanceType.IsValueType
                    ? HandleValueTypeNewObj(context, instruction, constructor, instanceType, arguments)
                    : HandleReferenceTypeNewObj(context, instruction, constructor, instanceType, arguments);
            }
            finally
            {
                for (int i = 0; i < arguments.Count; i++)
                    context.Machine.ValueFactory.BitVectorPool.Return(arguments[i]);
            }
        }

        private CilDispatchResult HandleValueTypeNewObj(
            CilExecutionContext context,
            CilInstruction instruction,
            IMethodDescriptor constructor,
            TypeSignature instanceType,
            IList<BitVector> arguments)
        {
            var factory = context.Machine.ValueFactory;
            var callerFrame = context.CurrentFrame;
            var callerStack = callerFrame.EvaluationStack;

            // Stack allocate the structure.
            long address = context.CurrentFrame.Allocate((int)factory.TypeManager.GetMethodTable(instanceType).ContentsLayout.Size);
            var thisPointer = factory.CreateNativeInteger(address);

            // Call the constructor with the constructor.
            arguments.Insert(0, thisPointer);
            var result = HandleCall(context, instruction, constructor, arguments);

            // If we stepped over the call, we need to push the stack value ourselves.
            if (result.IsSuccess && callerFrame == context.CurrentFrame)
                callerStack.Push(RetHandler.CreateResultingStackSlot(context, instanceType, thisPointer));

            return result;
        }

        private CilDispatchResult HandleReferenceTypeNewObj(
            CilExecutionContext context,
            CilInstruction instruction,
            IMethodDescriptor constructor,
            TypeSignature instanceType,
            IList<BitVector> arguments)
        {
            var callerFrame = context.CurrentFrame;
            var callerStack = callerFrame.EvaluationStack;

            var allocation = context.Machine.Allocator.Allocate(context, constructor, arguments);
            switch (allocation.ResultType)
            {
                case AllocationResultType.Inconclusive:
                    throw new CilEmulatorException(
                        $"Allocation of object of type {instanceType} was inconclusive");

                case AllocationResultType.Allocated:
                    // Insert the allocated "this" pointer into the arguments and call constructor.
                    arguments.Insert(0, allocation.Address!);
                    var result = HandleCall(context, instruction, constructor, arguments);

                    // If we stepped over the call, we need to push the stack value ourselves.
                    if (result.IsSuccess && callerFrame == context.CurrentFrame)
                        callerStack.Push(allocation.Address!, instanceType);

                    return result;

                case AllocationResultType.FullyConstructed:
                    // Fully constructed objects do not have to be post-processed.
                    callerStack.Push(allocation.Address!, instanceType);
                    callerFrame.ProgramCounter += instruction.Size;
                    return CilDispatchResult.Success();

                case AllocationResultType.Exception:
                    return CilDispatchResult.Exception(allocation.ExceptionObject);

                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        /// <inheritdoc />
        protected override bool ShouldPopInstanceObject(IMethodDescriptor method) => false;

        /// <inheritdoc />
        protected override MethodDevirtualizationResult DevirtualizeMethodInternal(CilExecutionContext context,
            IMethodDescriptor method,
            IList<BitVector> arguments)
        {
            return MethodDevirtualizationResult.Success(method);
        }
    }
}