using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
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
                long fieldAddress;

                if (field.Resolve() is {IsStatic: true})
                {
                    // Referenced field is static, we can ignore the instance object that was pushed.
                    fieldAddress = context.Machine.StaticFieldStorage.GetFieldAddress(field);
                }
                else if (instance.TypeHint != StackSlotTypeHint.Integer)
                {
                    throw new CilEmulatorException("Attempted to set a field on a non-object stack value.");
                }
                else
                {
                    // Object/structure was pushed by reference onto the stack. Dereference it.
                    var addressSpan = instance.Contents.AsSpan();

                    // We can only dereference fully known pointers.
                    if (!addressSpan.IsFullyKnown)
                    {
                        // TODO: Make configurable.
                        throw new CilEmulatorException("Attempted to write to an unknown object pointer.");
                    }

                    // Calculate field address.
                    long objectAddress = addressSpan.ReadNativeInteger(context.Machine.Is32Bit);
                    fieldAddress = objectAddress + factory.GetFieldMemoryLayout(field).Offset;

                    // Skip also the object header for fields defined within objects.
                    if (!field.DeclaringType!.IsValueType)
                        fieldAddress += factory.ObjectHeaderSize;
                }

                // Write field value.
                context.Machine.Memory.Write(fieldAddress, value);
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