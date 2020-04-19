using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a handler for instructions with the RET operation code.
    /// </summary>
    public class Ret : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ret
        };
        
        /// <inheritdoc />
        public DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            
            // Note that we do not actually pop the value here if the architecture says so. This is done intentionally
            // to allow the caller to obtain the return value from the stack.
            
            // We still want to check that the return is valid within the current context. That is, if it is expected
            // to return a value, the stack should contain exactly one value, and no values otherwise.
            
            int popCount = environment.Architecture.GetStackPopCount(instruction);
            if (context.ProgramState.Stack.Size != popCount)
            {
                return new DispatchResult
                {
                    Exception = new InvalidProgramException()
                };
            }
            
            return new DispatchResult
            {
                HasTerminated = true
            };
        }
    }
}