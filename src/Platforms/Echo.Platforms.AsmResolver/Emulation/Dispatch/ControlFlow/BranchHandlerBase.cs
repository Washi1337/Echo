using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    public abstract class BranchHandlerBase : ICilOpCodeHandler
    {
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            context.CurrentFrame.ProgramCounter = ((ICilLabel) instruction.Operand!).Offset;
            return CilDispatchResult.Success();
        }

        protected abstract bool EvaluateCondition(CilExecutionContext context, CilInstruction instruction);
    }
}