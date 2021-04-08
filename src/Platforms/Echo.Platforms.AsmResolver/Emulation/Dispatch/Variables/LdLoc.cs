using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Core.Code;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Variables
{
    /// <summary>
    /// Provides a handler for instructions with a variation of the <see cref="CilOpCodes.Ldloc"/> operation code.
    /// </summary>
    public class LdLoc : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ldloc, CilCode.Ldloc_0, CilCode.Ldloc_1, CilCode.Ldloc_2, CilCode.Ldloc_3, CilCode.Ldloc_S
        };

        /// <inheritdoc />
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            
            var variables = new IVariable[1];
            if (environment.Architecture.GetReadVariables(instruction, variables) != 1)
            {
                throw new DispatchException(
                    $"Architecture returned an incorrect number of variables being read from instruction {instruction}.");
            }
            
            switch (variables[0])
            {
                case CilVariable cilVariable:
                    var value = environment.CliMarshaller.ToCliValue(
                        context.ProgramState.Variables[variables[0]],
                        cilVariable.Variable.VariableType);
                    
                    context.ProgramState.Stack.Push(value);
                    return base.Execute(context, instruction);
                
                default:
                    return DispatchResult.InvalidProgram();
            }
        }
        
    }
}