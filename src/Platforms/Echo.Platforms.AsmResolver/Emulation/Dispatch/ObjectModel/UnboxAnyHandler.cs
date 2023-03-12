using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>unbox.any</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Unbox_Any)]
    public class UnboxAnyHandler : UnboxHandlerBase
    {
        /// <inheritdoc />
        protected override StackSlot GetReturnValue(CilExecutionContext context, ITypeDefOrRef type, long dataAddress)
        {
            var value = context.Machine.ValueFactory.RentValue(type.ToTypeSignature(true), false);
            context.Machine.Memory.Read(dataAddress, value);
            return new StackSlot(value, StackSlotTypeHint.Structure);
        }
    }
}