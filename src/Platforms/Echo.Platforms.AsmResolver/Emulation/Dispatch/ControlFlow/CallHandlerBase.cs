using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    public abstract class CallHandlerBase : ICilOpCodeHandler
    {
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
            var (result, stepType) = Invoke(context, devirtualization.ResultingMethod, arguments);

            // Move to the next instruction if call succeeded.
            if (result.IsSuccess)
                callerFrame.ProgramCounter += instruction.Size;
                
            return result;
        }

        private static IList<BitVector> PopArguments(CilExecutionContext context, IMethodDescriptor method)
        {
            var frame = context.CurrentFrame;
            int count = method.Signature!.GetTotalParameterCount();
            
            var arguments = new List<BitVector>(count);
            var marshaller = context.Machine.ValueFactory.Marshaller;

            int hasThisDelta = method.Signature.HasThis ? 1 : 0;
            int sentinelDelta = hasThisDelta + method.Signature.ParameterTypes.Count;

            var stack = frame.EvaluationStack;
            for (int i = 0; i < method.Signature.SentinelParameterTypes.Count; i++)
                arguments.Add(marshaller.FromCliValue(stack.Pop(), method.Signature.SentinelParameterTypes[i - sentinelDelta]));

            for (int i = 0; i < method.Signature.ParameterTypes.Count; i++)
                arguments.Add(marshaller.FromCliValue(stack.Pop(), method.Signature.ParameterTypes[i - hasThisDelta]));

            if (method.Signature.HasThis)
                arguments.Add(marshaller.FromCliValue(stack.Pop(), method.DeclaringType!.ToTypeSignature()));

            return arguments;
        }

        protected abstract MethodDevirtualizationResult DevirtualizeMethod(CilExecutionContext context,
            CilInstruction instruction,
            IList<BitVector> arguments);

        public (CilDispatchResult, StepType) Invoke(
            CilExecutionContext context, 
            IMethodDescriptor method,
            IList<BitVector> arguments)
        {
            if (context.Machine.InvocationStrategy.ShouldInvoke(context, method, arguments))
            {
                var result = context.Machine.Invoker.Invoke(context, method, arguments);
                if (result is not null)
                {
                    var stackSlot = context.Machine.ValueFactory.Marshaller.ToCliValue(result, method.Signature!.ReturnType);
                    context.CurrentFrame.EvaluationStack.Push(stackSlot);
                }

                return (CilDispatchResult.Success(), StepType.StepOver);
            }

            var frame = context.Machine.CallStack.Push(method);
            
            for (int i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];
                frame.WriteArgument(i, argument.AsSpan());
            }
            
            return (CilDispatchResult.Success(), StepType.StepIn);
        }
    }
}
