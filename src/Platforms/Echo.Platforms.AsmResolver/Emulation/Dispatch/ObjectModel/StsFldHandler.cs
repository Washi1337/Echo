using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>stsfld</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Stsfld)]
    public class StsFldHandler : FieldOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(
            CilExecutionContext context, 
            CilInstruction instruction, 
            IFieldDescriptor field)
        {
            var value = context.CurrentFrame.EvaluationStack.Pop(field.Signature!.FieldType);
            
            try
            {
                var fieldSpan = context.Machine.StaticFields.GetFieldSpan(field);
                fieldSpan.Write(value);

                return CilDispatchResult.Success();
            }
            finally
            {
                context.Machine.ValueFactory.BitVectorPool.Return(value);
            }
        }
    }
}