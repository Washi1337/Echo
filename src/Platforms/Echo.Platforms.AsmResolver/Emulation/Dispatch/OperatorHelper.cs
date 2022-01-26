using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    internal static class OperatorHelper
    {
        public static (StackSlot Left, StackSlot Right) PopBinaryArguments(CilExecutionContext context, bool signed)
        {
            var argument2 = context.CurrentFrame.EvaluationStack.Pop();
            var argument1 = context.CurrentFrame.EvaluationStack.Pop();

            // Resize bitvectors if required.
            var pool = context.Machine.ValueFactory.BitVectorPool;
            if (argument1.Contents.Count != argument2.Contents.Count)
            {
                if (argument1.Contents.Count < argument2.Contents.Count)
                {
                    var newArgument1 = new StackSlot(
                        argument1.Contents.Resize(argument2.Contents.Count, signed, pool),
                        StackSlotTypeHint.Integer);
                    pool.Return(argument1.Contents);
                    argument1 = newArgument1;
                }
                else
                {
                    var newArgument2 = new StackSlot(
                        argument2.Contents.Resize(argument1.Contents.Count, signed, pool),
                        StackSlotTypeHint.Integer);
                    pool.Return(argument2.Contents);
                    argument2 = newArgument2;
                }
            }

            return (argument1, argument2);
        }
    }
}