using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Ret"/> operation code.
    /// </summary>
    public class Ret : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ret
        };
        
        /// <inheritdoc />
        public DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            
            // If the containing method is expected to return a value, the stack should contain exactly one value,
            // and no values otherwise.
            
            int popCount = environment.Architecture.GetStackPopCount(instruction);
            if (context.ProgramState.Stack.Size != popCount)
                return DispatchResult.InvalidProgram();

            // Pop result.
            if (popCount == 1)
                context.Result.ReturnValue = context.ProgramState.Stack.Pop();
            
            return new DispatchResult
            {
                HasTerminated = true
            };
        }
    }
}