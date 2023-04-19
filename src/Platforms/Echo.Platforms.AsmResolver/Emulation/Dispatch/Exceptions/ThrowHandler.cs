using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Exceptions
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>throw</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Throw)]
    public class ThrowHandler : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var pool = context.Machine.ValueFactory.BitVectorPool;

            var value = stack.Pop();
            stack.Clear();

            try
            {
                if (value.TypeHint != StackSlotTypeHint.Integer)
                    return CilDispatchResult.InvalidProgram(context);

                if (value.Contents.AsSpan().IsZero)
                    return CilDispatchResult.NullReference(context);

                var exceptionObject = value.Contents.AsObjectHandle(context.Machine);
                
                // TODO: set stack trace.
                
                return CilDispatchResult.Exception(exceptionObject);
            }
            finally
            {
                pool.Return(value.Contents);
            }
        }
    }
}