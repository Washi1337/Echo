using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Stsfld"/> operation code.
    /// </summary>
    public class StsFld : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Stsfld
        };

        /// <inheritdoc />
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            
            var referencedField = (IFieldDescriptor) instruction.Operand;
            var staticField = environment.StaticFieldFactory.Get(referencedField);
            
            var value = (ICliValue) context.ProgramState.Stack.Pop();
            staticField.Value = environment.CliMarshaller.ToCtsValue(value, referencedField.Signature.FieldType);
            
            return base.Execute(context, instruction);
        }
    }
}