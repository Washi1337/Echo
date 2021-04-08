using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Constants
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Ldstr"/> operation code.
    /// </summary>
    public class Ldstr : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ldstr
        };

        /// <inheritdoc />
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var marshaller = environment.CliMarshaller;
            
            string rawValue = (string) instruction.Operand;
            var cliValue = marshaller.ToCliValue(
                environment.ValueFactory.GetStringValue(rawValue),
                environment.Module.CorLibTypeFactory.String);
            
            context.ProgramState.Stack.Push(cliValue);
            return base.Execute(context, instruction);
        }
    }
}