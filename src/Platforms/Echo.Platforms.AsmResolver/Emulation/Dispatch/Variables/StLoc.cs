using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Core.Code;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

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
            
            var variables = new IVariable[1];
            if (environment.Architecture.GetWrittenVariables(instruction, variables) != 1)
            {
                throw new DispatchException(
                    $"Architecture returned an incorrect number of variables being written by instruction {instruction}.");
            }
            
            switch (variables[0])
            {
                case CilVariable cilVariable:
                    var value = environment.CliMarshaller.ToCtsValue(
                        (ICliValue) context.ProgramState.Stack.Pop(),
                        cilVariable.Variable.VariableType);

                    context.ProgramState.Variables[variables[0]] = value;
                    return base.Execute(context, instruction);
                
                default:
                    return DispatchResult.InvalidProgram();
            }
        }
        
    }
}