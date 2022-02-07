using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides a base implementation for operation codes that call functions or methods.
    /// </summary>
    public abstract class CallHandlerBase : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            var method = (IMethodDescriptor) instruction.Operand!;
            var arguments = PopArguments(context, method);

            var callerFrame = context.CurrentFrame;

            // Devirtualize the method in the operand.
            var devirtualization = DevirtualizeMethod(context, instruction, arguments);
            if (devirtualization.IsUnknown)
                throw new CilEmulatorException($"Could not devirtualize method call {instruction}.");
            
            // If that resulted in any error, throw it.
            if (!devirtualization.IsSuccess)
                return CilDispatchResult.Exception(new BitVector((ulong) devirtualization.ExceptionPointer));
            
            // Invoke it otherwise.
            var result = Invoke(context, devirtualization.ResultingMethod, arguments);

            // Move to the next instruction if call succeeded.
            if (result.IsSuccess)
                callerFrame.ProgramCounter += instruction.Size;
                
            return result;
        }

        private static IList<BitVector> PopArguments(CilExecutionContext context, IMethodDescriptor method)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;
            var marshaller = factory.Marshaller;

            var arguments = new List<BitVector>(method.Signature!.GetTotalParameterCount());
            
            // Pop sentinel arguments.
            for (int i = 0; i < method.Signature.SentinelParameterTypes.Count; i++)
                arguments.Add(marshaller.FromCliValue(stack.Pop(), method.Signature.SentinelParameterTypes[i]));

            // Pop normal arguments.
            for (int i = 0; i < method.Signature.ParameterTypes.Count; i++)
                arguments.Add(marshaller.FromCliValue(stack.Pop(), method.Signature.ParameterTypes[i]));

            // Pop instance object.
            if (method.Signature.HasThis)
            {
                var declaringType = method.DeclaringType?.ToTypeSignature() ?? factory.ContextModule.CorLibTypeFactory.Object;
                arguments.Add(marshaller.FromCliValue(stack.Pop(), declaringType));
            }

            // Correct for stack order.
            arguments.Reverse();

            return arguments;
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
            // Check if we should invoke this call, or whether we should step in.
            if (context.Machine.InvocationStrategy.ShouldInvoke(context, method, arguments))
            {
                // Invoke the method, and push the result if it produced a value.
                var result = context.Machine.Invoker.Invoke(context, method, arguments);
                if (!result.IsSuccess)
                    return CilDispatchResult.Exception(result.Value);
                
                if (result.Value is not null)
                {
                    var stackSlot = context.Machine.ValueFactory.Marshaller.ToCliValue(result.Value, method.Signature!.ReturnType);
                    context.CurrentFrame.EvaluationStack.Push(stackSlot);
                }

                return CilDispatchResult.Success();
            }

            // We are stepping into this method, push new call frame with the arguments that were pushed onto the stack.
            var frame = context.Machine.CallStack.Push(method);
            for (int i = 0; i < arguments.Count; i++)
                frame.WriteArgument(i, arguments[i].AsSpan());

            return CilDispatchResult.Success();
        }
    }
}
