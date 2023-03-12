using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>unbox</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Unbox)]
    public class UnboxHandler : UnboxHandlerBase
    {
        /// <inheritdoc />
        protected override StackSlot GetReturnValue(CilExecutionContext context, ITypeDefOrRef type, long dataAddress)
        {
            return new StackSlot(dataAddress, StackSlotTypeHint.Integer);
        }
    }
}