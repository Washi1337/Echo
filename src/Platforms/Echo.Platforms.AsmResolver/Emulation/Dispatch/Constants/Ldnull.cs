using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Constants
{
    public class Ldnull : FallThroughOpCodeHandler
    {
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ldnull,
        };

        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            context.ProgramState.Stack.Push(OValue.Null(environment.Is32Bit));
            return base.Execute(context, instruction);
        }
    }
}