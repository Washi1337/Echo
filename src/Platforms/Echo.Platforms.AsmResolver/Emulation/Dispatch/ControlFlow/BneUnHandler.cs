using AsmResolver.PE.DotNet.Cil;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>bne.un</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Bne_Un, CilCode.Bne_Un_S)]
    public class BneUnHandler : BinaryBranchHandlerBase
    {
        /// <inheritdoc />
        protected override bool IsSignedCondition(CilInstruction instruction) => false;

        /// <inheritdoc />
        protected override Trilean EvaluateCondition(CilInstruction instruction, 
            StackSlot argument1,
            StackSlot argument2)
        {
            return !argument1.Contents.AsSpan().IsEqualTo(argument2.Contents.AsSpan());
        }
    }
}