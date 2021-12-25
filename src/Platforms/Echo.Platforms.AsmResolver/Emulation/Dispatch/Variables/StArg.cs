using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Core.Code;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Variables
{
    /// <summary>
    /// Provides a handler for instructions with a variation of the <see cref="CilOpCodes.Stloc"/> operation code.
    /// </summary>
    public class StArg : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Starg, CilCode.Starg_S
        };

        /// <inheritdoc />
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            
            var variables = new IVariable[1];
            if (environment.Architecture.GetWrittenVariables(instruction, variables) != 1)
            {
                throw new DispatchException(
                    $"Architecture returned an incorrect number of variables being written by instruction {instruction}.");
            }
            
            switch (variables[0])
            {
                case CilParameter parameter:
                    var value = environment.CliMarshaller.ToCtsValue(
                        context.ProgramState.Stack.Pop(),
                        parameter.Parameter.ParameterType);

                    context.ProgramState.Variables[variables[0]] = value;
                    return base.Execute(context, instruction);
                
                default:
                    return DispatchResult.InvalidProgram();
            }
        }
        
    }
}