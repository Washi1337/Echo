using System;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldfld</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ldfld)]
    public class LdFldHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;
            
            var field = (IFieldDescriptor) instruction.Operand!;
            var instance = stack.Pop();
            var result = context.Machine.ValueFactory.RentValue(field.Signature!.FieldType, false);

            try
            {
                // We can actually reference static fields with ldfld. The instance is then just ignored.
                if (field.Resolve() is {IsStatic: true})
                    result.AsSpan().Write(context.Machine.StaticFieldStorage.GetFieldSpan(field));
                else
                    ReadInstanceField(context, instance, field, result);

                // Push.
                stack.Push(result, field.Signature!.FieldType);
            }
            finally
            {
                factory.BitVectorPool.Return(instance.Contents);
                factory.BitVectorPool.Return(result);
            }

            return CilDispatchResult.Success();
        }

        private static void ReadInstanceField(
            CilExecutionContext context, 
            StackSlot instance,
            IFieldDescriptor field,
            BitVector result)
        {
            var factory = context.Machine.ValueFactory;
            
            switch (instance.TypeHint)
            {
                case StackSlotTypeHint.Structure:
                    // Structure was pushed onto the stack directly. Read the field directly from the structure.
                    result.AsSpan().Write(instance.Contents.AsSpan().SliceStructField(factory, field));
                    break;

                case StackSlotTypeHint.Float:
                    // Floats should not be able to contain a pointer to an object or a structure with fields.
                    throw new CilEmulatorException("Attempted to dereference a floating point number.");

                case StackSlotTypeHint.Integer:
                    // Object/structure was pushed by reference onto the stack. Dereference it.
                    var addressSpan = instance.Contents.AsSpan();
                    if (!addressSpan.IsFullyKnown)
                    {
                        // We can only dereference fully known pointers. Leave the result unknown.
                    }
                    else
                    {
                        // Calculate field address.
                        long objectAddress = addressSpan.ReadNativeInteger(context.Machine.Is32Bit);
                        long fieldAddress = factory.GetFieldAddress(objectAddress, field);

                        // Read field value.
                        context.Machine.Memory.Read(fieldAddress, result);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}