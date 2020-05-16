using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    public class Call : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Call
        };

        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();

            var method = (IMethodDescriptor) instruction.Operand;
            bool pushesValue = environment.Architecture.GetStackPushCount(instruction) > 0;

            int argumentCount = environment.Architecture.GetStackPopCount(instruction);
            var arguments = context.ProgramState.Stack
                .Pop(argumentCount, true)
                .Cast<ICliValue>();
            
            var result = environment.MethodInvoker.Invoke(method, arguments);

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
        
    }
}