using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Core.Code;

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
                case CilParameter parameter:
                    var value = environment.CliMarshaller.ToCliValue(
                        context.ProgramState.Variables[variables[0]],
                        parameter.Parameter.ParameterType);
                    
                    context.ProgramState.Stack.Push(value);
                    return base.Execute(context, instruction);
                
                default:
                    return DispatchResult.InvalidProgram();
            }
        }
        
    }
}