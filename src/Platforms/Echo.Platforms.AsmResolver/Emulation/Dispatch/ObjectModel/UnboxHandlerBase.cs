using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides a base for instructions implementing unboxing behavior.
    /// </summary>
    public abstract class UnboxHandlerBase : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var type = (ITypeDefOrRef) instruction.Operand!;

            var stack = context.CurrentFrame.EvaluationStack;

            var address = stack.Pop();
            try
            {
                var addressSpan = address.Contents.AsSpan();
                switch (addressSpan)
                {
                    case { IsFullyKnown: false }:
                        // TODO: make configurable.
                        throw new CilEmulatorException("Attempted to dereference an unknown pointer.");
                    
                    case { IsZero.Value: TrileanValue.True }:
                        return CilDispatchResult.NullReference(context);
                    
                    default:
                        var actualType = addressSpan.GetObjectPointerType(context.Machine);
                        
                        // TODO: type checks.

                        long dataAddress = addressSpan.ReadNativeInteger(context.Machine.Is32Bit) 
                                           + context.Machine.ValueFactory.ObjectHeaderSize;
                        stack.Push(GetReturnValue(context, type, dataAddress));
                        return CilDispatchResult.Success();
                }
            }
            finally
            {
                context.Machine.ValueFactory.BitVectorPool.Return(address.Contents);
            }
        }

        /// <summary>
        /// Transforms the resolved data address into a value to be pushed onto the stack.
        /// </summary>
        /// <param name="context">The context in which the instruction is emulated in.</param>
        /// <param name="type">The data type of the value at the address.</param>
        /// <param name="dataAddress">The address.</param>
        /// <returns>The return value.</returns>
        protected abstract StackSlot GetReturnValue(CilExecutionContext context, ITypeDefOrRef type, long dataAddress);
    }

}