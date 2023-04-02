using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>stfld</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Stfld)]
    public class StFldHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;
            
            var field = (IFieldDescriptor) instruction.Operand!;
            var value = stack.Pop(field.Signature!.FieldType);
            var instance = stack.Pop();

            try
            {
                if (field.Resolve() is {IsStatic: true})
                {
                    // Referenced field is static, we can ignore the instance object that was pushed.
                    long fieldAddress = context.Machine.StaticFields.GetFieldAddress(field);
                    context.Machine.Memory.Write(fieldAddress, value);
                }
                else if (instance.TypeHint != StackSlotTypeHint.Integer)
                {
                    throw new CilEmulatorException("Attempted to set a field on a non-object stack value.");
                }
                else
                {
                    // Object/structure was pushed by reference onto the stack. Dereference it.
                    long? objectAddress = instance.Contents.IsFullyKnown
                        ? instance.Contents.AsSpan().ReadNativeInteger(context.Machine.Is32Bit)
                        : context.Machine.UnknownResolver.ResolveDestinationPointer(context, instruction, instance);

                    switch (objectAddress)
                    {
                        case null:
                            // If address is unknown even after resolution, assume it writes to "somewhere" successfully.
                            return CilDispatchResult.Success();

                        case 0:
                            // A null reference was passed.
                            return CilDispatchResult.NullReference(context);

                        case { } actualAddress:
                            // A non-null reference was passed.
                            var handle = field.DeclaringType!.IsValueType
                                ? actualAddress.AsStructHandle(context.Machine)
                                : actualAddress.AsObjectHandle(context.Machine).Contents;

                            handle.WriteField(field, value);
                            break;
                    }
                }

            }
            finally
            {
                factory.BitVectorPool.Return(instance.Contents);
                factory.BitVectorPool.Return(value);
            }

            return CilDispatchResult.Success();
        }
    }
}