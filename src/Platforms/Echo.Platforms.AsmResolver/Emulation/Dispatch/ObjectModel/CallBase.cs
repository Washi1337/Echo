using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides a base for operation code handlers that call procedures outside of the method body. 
    /// </summary>
    public abstract class CallBase : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();

            // Pop arguments.
            int argumentCount = environment.Architecture.GetStackPopCount(instruction);
            var arguments = context.ProgramState.Stack
                .Pop(argumentCount, true)
                .Cast<ICliValue>()
                .ToList();

            // Dispatch
            var methodDispatch = DevirtualizeMethod(instruction, arguments);
            if (methodDispatch.Exception != null)
                return new DispatchResult(methodDispatch.Exception);

            // Invoke.
            var result = InvokeAndGetResult(environment, instruction, methodDispatch, arguments);

            // Push result if necessary.
            UpdateStack(context, result, instruction);

            return base.Execute(context, instruction);
        }

        private static ICliValue InvokeAndGetResult(ICilRuntimeEnvironment environment, CilInstruction instruction,
            MethodDevirtualizationResult methodDispatch, List<ICliValue> arguments)
        {
            if (methodDispatch.IsUnknown)
            {
                if (instruction.Operand is IMethodDescriptor method)
                {
                    return environment.CliMarshaller.ToCliValue(
                        environment.UnknownValueFactory.CreateUnknown(method.Signature.ReturnType),
                        method.Signature.ReturnType);
                }

                throw new DispatchException("Operand of call instruction is not a method.");
            }

            if (methodDispatch.ResultingMethod != null)
                return environment.MethodInvoker.Invoke(methodDispatch.ResultingMethod, arguments);

            if (methodDispatch.ResultingMethodSignature != null)
            {
                var address = arguments.Last();
                arguments.Remove(address);
                return environment.MethodInvoker.InvokeIndirect(address, methodDispatch.ResultingMethodSignature,
                    arguments);
            }

            return null;
        }

        private static void UpdateStack(ExecutionContext context, ICliValue result, CilInstruction cilInstruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            
            bool pushesValue = environment.Architecture.GetStackPushCount(cilInstruction) > 0;
            if (result is null)
            {
                if (pushesValue)
                {
                    throw new DispatchException(
                        "Method was expected to return a value, but the method invoker returned a null value.");
                }
            }
            else
            {
                if (!pushesValue)
                {
                    throw new DispatchException(
                        "Method was not expected to return a value, but the method invoker returned a non-null value.");
                }

                context.ProgramState.Stack.Push(result);
            }
        }

        /// <summary>
        /// Devirtualizes the method referenced by the provided instruction, and infers the actual method
        /// implementing the referenced method that was called.
        /// </summary>
        /// <param name="instruction">The call instruction.</param>
        /// <param name="arguments">The arguments of the method call.</param>
        /// <returns>The result of the devirtualization process.</returns>
        protected abstract MethodDevirtualizationResult DevirtualizeMethod(
            CilInstruction instruction,
            IList<ICliValue> arguments);
    }
}