using System.Collections.Generic;
using System.Linq;
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
            bool pushesValue = environment.Architecture.GetStackPushCount(instruction) > 0;
            var result = environment.MethodInvoker.Invoke(methodDispatch.ResultingMethod, arguments);

            // Push result if necessary.
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

            return base.Execute(context, instruction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        protected abstract MethodDevirtualizationResult DevirtualizeMethod(
            CilInstruction instruction,
            IList<ICliValue> arguments);
    }
}