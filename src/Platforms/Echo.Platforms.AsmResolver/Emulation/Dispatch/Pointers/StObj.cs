using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values.ReferenceType;
using Echo.Platforms.AsmResolver.Emulation.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Pointers
{
    /// <summary>
    /// Provides a handler for instructions with a variation of the <see cref="CilOpCodes.Stobj"/> operation code.
    /// </summary>
    public class StObj : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Stobj
        };

        /// <inheritdoc />
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var stack = context.ProgramState.Stack;

            // Pop address to dereference.
            var elementValue = stack.Pop();
            var addressValue = stack.Pop();

            if (!(addressValue is IPointerValue pointerValue))
                return DispatchResult.InvalidProgram();

            // Determine type layout.
            var type = ((ITypeDefOrRef) instruction.Operand).ToTypeSignature();
            var memoryLayout = environment.ValueFactory.GetTypeMemoryLayout(type);

            // Write
            pointerValue.WriteStruct(0, environment.ValueFactory, memoryLayout, elementValue);

            return DispatchResult.Success();
        }
    }
}