using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>sizeof</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Sizeof)]
    public class SizeOfHandler : FallThroughOpCodeHandler 
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;
            var genericContext = GenericContext.FromMethod(context.CurrentFrame.Method);

            var type = ((ITypeDefOrRef)instruction.Operand!).ToTypeSignature().InstantiateGenericTypes(genericContext);
            var value = factory.BitVectorPool.Rent(32, false);
            value.AsSpan().Write(factory.GetTypeValueMemoryLayout(type).Size);
            stack.Push(new StackSlot(value, StackSlotTypeHint.Integer));

            return CilDispatchResult.Success();
        }
    }
}