using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldsfld</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ldsfld)]
    public class LdsFldHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var field = (IFieldDescriptor) instruction.Operand!;
            var fieldSpan = context.Machine.StaticFields.GetFieldSpan(field);
            context.CurrentFrame.EvaluationStack.Push(fieldSpan, field.Signature!.FieldType);
            return CilDispatchResult.Success();
        }
    }
}