using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides a base implementation for operation codes that call functions or methods.
    /// </summary>
    public abstract class CallHandlerBase : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public virtual CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            var method = (IMethodDescriptor) instruction.Operand!;
            var arguments = GetArguments(context, method);

            try
            {
                var callerFrame = context.CurrentFrame;

                // Devirtualize the method in the operand.
                var devirtualization = DevirtualizeMethod(context, instruction, arguments);
                if (devirtualization.IsUnknown)
                    throw new CilEmulatorException($"Could not devirtualize method call {instruction}.");

                // If that resulted in any error, throw it.
                if (!devirtualization.IsSuccess)
                    return CilDispatchResult.Exception(new BitVector(devirtualization.ExceptionPointer.Value));

                // Invoke it otherwise.
                var result = Invoke(context, devirtualization.ResultingMethod, arguments);

                // Move to the next instruction if call succeeded.
                if (result.IsSuccess)
                    callerFrame.ProgramCounter += instruction.Size;
                
                return result;
            }
            finally
            {
                for (int i = 0; i < arguments.Count; i++)
                    context.Machine.ValueFactory.BitVectorPool.Return(arguments[i]);
            }
        }

        private IList<BitVector> GetArguments(CilExecutionContext context, IMethodDescriptor method)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var result = new List<BitVector>(method.Signature!.GetTotalParameterCount());
            
            // Pop sentinel arguments.
            for (int i = 0; i < method.Signature.SentinelParameterTypes.Count; i++)
                result.Add(stack.Pop(method.Signature.SentinelParameterTypes[i]));

            // Pop normal arguments.
            for (int i = 0; i < method.Signature.ParameterTypes.Count; i++)
                result.Add(stack.Pop(method.Signature.ParameterTypes[i]));

            // Pop instance object.
            if (method.Signature.HasThis)
                result.Add(GetInstancePointer(context, method));

            // Correct for stack order.
            result.Reverse();

            return result;
        }

        private BitVector GetInstancePointer(CilExecutionContext context, IMethodDescriptor method)
        {
            var factory = context.Machine.ValueFactory;
            var stack = context.CurrentFrame.EvaluationStack;
            
            var declaringType = method.DeclaringType?.ToTypeSignature() ?? factory.ContextModule.CorLibTypeFactory.Object;
            return stack.Pop(declaringType);
        }

        /// <summary>
        /// Devirtualizes and resolves the method that is referenced by the provided instruction.
        /// </summary>
        /// <param name="context">The execution context the instruction is evaluated in.</param>
        /// <param name="instruction">The instruction that is being evaluated.</param>
        /// <param name="arguments">The arguments pushed onto the stack.</param>
        /// <returns>The result of the devirtualization.</returns>
        protected abstract MethodDevirtualizationResult DevirtualizeMethod(
            CilExecutionContext context,
            CilInstruction instruction,
            IList<BitVector> arguments);

        private static CilDispatchResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            var result = context.Machine.Invoker.Invoke(context, method, arguments);

            switch (result.ResultType)
            {
                case InvocationResultType.StepIn:
                    // We are stepping into this method, push new call frame with the arguments that were pushed onto the stack.
                    var frame = new CallFrame(method, context.Machine.ValueFactory);
                    for (int i = 0; i < arguments.Count; i++)
                        frame.WriteArgument(i, arguments[i]);

                    context.Machine.CallStack.Push(frame);
                    return CilDispatchResult.Success();

                case InvocationResultType.StepOver:
                    // Method was fully handled by the invoker, push result if it produced any.
                    if (result.Value is not null)
                        context.CurrentFrame.EvaluationStack.Push(result.Value, method.Signature!.ReturnType, true);

                    return CilDispatchResult.Success();

                case InvocationResultType.Exception:
                    // There was an exception during the invocation. Throw it.
                    return CilDispatchResult.Exception(result.Value!);

                case InvocationResultType.Inconclusive:
                    // Method invoker was not able to handle the method call.
                    throw new CilEmulatorException($"Invocation of method {method} was inconclusive.");

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}