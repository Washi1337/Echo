using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    public abstract class FallThroughOpCodeHandler : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            var result = DispatchInternal(context, instruction);
            if (result.IsSuccess)
                context.CurrentFrame.ProgramCounter += instruction.Size;
            return result;
        }

        protected abstract CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction);
    }
}