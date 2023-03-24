using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>throw</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Throw)]
    public class ThrowHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var pool = context.Machine.ValueFactory.BitVectorPool;

            var value = stack.Pop();

            try
            {
                if (value.TypeHint != StackSlotTypeHint.Integer)
                    return CilDispatchResult.InvalidProgram(context);

                if (value.Contents.AsSpan().IsZero)
                    return CilDispatchResult.NullReference(context);

                return CilDispatchResult.Exception(value.Contents.Clone(pool));
            }
            finally
            {
                pool.Return(value.Contents);
            }
        }
    }
}