using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ret</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ret)]
    public class RetHandler : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            var calleeFrame = context.Thread.CallStack.Pop();
            var callerFrame = context.CurrentFrame;

            var genericContext = GenericContext.FromMethod(calleeFrame.Method);
            if (calleeFrame.Method.Signature!.ReturnsValue)
            {
                // The method returns the value on top of the stack. Push it in the caller frame.
                var returnType = calleeFrame.Method.Signature.ReturnType.InstantiateGenericTypes(genericContext);
                var value = calleeFrame.EvaluationStack.Pop(returnType);
                callerFrame.EvaluationStack.Push(value, returnType, true);
            }
            else if (callerFrame.Body is { } body)
            {
                // The method may still be a constructor called via newobj.
                // In that case we need to push the created value, stored in the `this` pointer.

                int index = body.Instructions.GetIndexByOffset(callerFrame.ProgramCounter) - 1;
                if (index != -1 && body.Instructions[index].OpCode.Code == CilCode.Newobj)
                {
                    var resultingType = calleeFrame.Method.DeclaringType!
                        .ToTypeSignature()
                        .InstantiateGenericTypes(genericContext);

                    var slot = CreateResultingStackSlot(
                        context,
                        resultingType,
                        calleeFrame.ReadArgument(0)
                    );

                    callerFrame.EvaluationStack.Push(slot);
                }
            }

            return CilDispatchResult.Success();
        }

        internal static StackSlot CreateResultingStackSlot(
            CilExecutionContext context,
            TypeSignature type,
            BitVectorSpan thisPointer)
        {
            // For reference types, we can just wrap the address into a stack slot.
            if (!type.IsValueType)
            {
                return new StackSlot(
                    thisPointer.ToVector(context.Machine.ValueFactory.BitVectorPool),
                    StackSlotTypeHint.Integer
                );
            }

            // For value types, we need to push the resulting structure in its entirety.
            var contents = context.Machine.ValueFactory.CreateValue(type, false);

            // Read the entire structure behind the this-pointer.
            if (thisPointer.IsFullyKnown)
            {
                context.Machine.Memory.Read(
                    thisPointer.ReadNativeInteger(context.Machine.Is32Bit),
                    contents
                );
            }

            return new StackSlot(contents, StackSlotTypeHint.Structure);
        }
    }
}