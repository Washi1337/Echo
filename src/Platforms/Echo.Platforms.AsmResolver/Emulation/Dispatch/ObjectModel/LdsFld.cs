using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Ldsfld"/> operation code.
    /// </summary>
    public class LdsFld : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ldsfld
        };

        /// <inheritdoc />
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            
            var referencedField = (IFieldDescriptor) instruction.Operand;
            var staticField = environment.StaticFieldFactory.Get(referencedField);
            var value = environment.CliMarshaller.ToCliValue(staticField.Value, referencedField.Signature.FieldType);
            context.ProgramState.Stack.Push(value);
            
            return base.Execute(context, instruction);
        }
    }
}