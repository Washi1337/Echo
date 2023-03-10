using AsmResolver.PE.DotNet.Cil;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldlen</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ldlen)]
    public class LdLenHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;
            
            var arrayAddress = stack.Pop().Contents;
            var result = factory.RentNativeInteger(false);
            
            try
            {
                var arrayAddressSpan = arrayAddress.AsSpan();
                switch (arrayAddressSpan)
                {
                    case { IsFullyKnown: false }:
                        break;

                    case { IsZero.Value: TrileanValue.True }:
                        return CilDispatchResult.NullReference(context);

                    default:
                        var arraySpan = context.Machine.Heap.GetObjectSpan(arrayAddressSpan.ReadNativeInteger(context.Machine.Is32Bit));
                        result.AsSpan().Write(arraySpan.SliceArrayLength(factory));
                        break;
                }

                stack.Push(new StackSlot(result, StackSlotTypeHint.Integer));
                return CilDispatchResult.Success();
            }
            finally
            {
                factory.BitVectorPool.Return(arrayAddress);
            }
        }
    }
}