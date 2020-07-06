using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Variables
{
    /// <summary>
    /// Provides a handler for instructions with a variation of the <see cref="CilOpCodes.Ldarg"/> operation code.
    /// </summary>
    public class LdArg : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ldarg, CilCode.Ldarg_0, CilCode.Ldarg_1, CilCode.Ldarg_2, CilCode.Ldarg_3, CilCode.Ldarg_S
        };

        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var variable = environment.Architecture
                .GetReadVariables(instruction)
                .First();

            switch (variable)
            {
                case CilParameter parameter:
                    var value = environment.CliMarshaller.ToCliValue(
                        context.ProgramState.Variables[variable],
                        parameter.Parameter.ParameterType);
                    
                    context.ProgramState.Stack.Push(value);
                    return base.Execute(context, instruction);
                
                default:
                    return DispatchResult.InvalidProgram();
            }
        }
        
    }
}