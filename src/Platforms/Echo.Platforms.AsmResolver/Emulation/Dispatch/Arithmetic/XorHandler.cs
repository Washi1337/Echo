using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    [DispatcherTableEntry(CilCode.Xor)]
    public class XorHandler : BinaryOpCodeHandlerBase
    {
        protected override bool IsSignedOperation(CilInstruction instruction) => false;

        protected override CilDispatchResult Evaluate(CilExecutionContext context, CilInstruction instruction, 
            StackSlot argument1, StackSlot argument2)
        {
            argument1.Contents.AsSpan().Xor(argument2.Contents.AsSpan());
            return CilDispatchResult.Success();
        }
    }
}