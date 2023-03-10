using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arithmetic
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ceq</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ceq)]
    public class CeqHandler : BinaryOperatorHandlerBase
    {
        /// <inheritdoc />
        protected override bool Force32BitResult(CilInstruction instruction) => true;

        /// <inheritdoc />
        protected override bool IsSignedOperation(CilInstruction instruction) => false;

        /// <inheritdoc />
        protected override CilDispatchResult Evaluate(
            CilExecutionContext context, 
            CilInstruction instruction, 
            StackSlot argument1, 
            StackSlot argument2)
        {
            var argument1Span = argument1.Contents.AsSpan();
            
            var result = argument1Span.IsEqualTo(argument2.Contents.AsSpan());
            argument1Span.Clear();
            argument1Span[0] = result;
            
            return CilDispatchResult.Success();
        }
    }
}