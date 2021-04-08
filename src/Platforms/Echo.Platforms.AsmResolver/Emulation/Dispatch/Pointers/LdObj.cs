using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Pointers
{
    /// <summary>
    /// Provides a handler for instructions with a variation of the <see cref="CilOpCodes.Ldobj"/> operation code.
    /// </summary>
    public class LdObj : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ldobj
        };

        /// <inheritdoc />
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var stack = context.ProgramState.Stack;

            // Pop address to dereference.
            var addressValue = stack.Pop();
            if (!(addressValue is PointerValue pointerValue))
                return DispatchResult.InvalidProgram();

            // Determine type layout.
            var type = ((ITypeDefOrRef) instruction.Operand).ToTypeSignature();
            var memoryLayout = environment.ValueFactory.GetTypeMemoryLayout(type);

            // Dereference.
            var structureValue = pointerValue.ReadStruct(0, environment.ValueFactory, memoryLayout);
            stack.Push(environment.CliMarshaller.ToCliValue(structureValue, type));
            
            return DispatchResult.Success();
        }
        
    }
}