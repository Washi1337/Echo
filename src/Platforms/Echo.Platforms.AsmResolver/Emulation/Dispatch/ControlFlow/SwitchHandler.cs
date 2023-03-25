using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>switch</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Switch)]
    public class SwitchHandler : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            var index = context.CurrentFrame.EvaluationStack.Pop();

            try
            {
                var indexSpan = index.Contents.AsSpan();

                uint? selectedIndex = indexSpan.IsFullyKnown 
                    ? indexSpan.U32 
                    : context.Machine.UnknownResolver.ResolveSwitchCondition(context, instruction, index);

                var labels = (IList<ICilLabel>) instruction.Operand!;
                if (selectedIndex is null || selectedIndex >= labels.Count)
                    context.CurrentFrame.ProgramCounter += instruction.Size;
                else
                    context.CurrentFrame.ProgramCounter = labels[(int) selectedIndex].Offset;
            }
            finally
            {
                context.Machine.ValueFactory.BitVectorPool.Return(index.Contents);
            }

            return CilDispatchResult.Success();
        }
    }
}