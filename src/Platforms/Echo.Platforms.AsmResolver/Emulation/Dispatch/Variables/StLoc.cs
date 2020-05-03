using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Variables
{
    /// <summary>
    /// Provides a handler for instructions with a variation of the <see cref="CilOpCodes.Stloc"/> operation code.
    /// </summary>
    public class StLoc : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Stloc, CilCode.Stloc_0, CilCode.Stloc_1, CilCode.Stloc_2, CilCode.Stloc_3, CilCode.Stloc_S
        };

        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var variable = environment.Architecture
                .GetWrittenVariables(instruction)
                .First();
            
            if (!(variable is CilVariable))
                return DispatchResult.InvalidProgram();
            
            context.ProgramState.Variables[variable] = context.ProgramState.Stack.Pop();
            return base.Execute(context, instruction);
        }
    }
}