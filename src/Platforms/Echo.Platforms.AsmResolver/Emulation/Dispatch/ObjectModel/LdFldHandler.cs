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
                            long fieldAddress = objectAddress + factory.GetFieldMemoryLayout(field).Offset;
                            
                            // Skip also the object header for fields defined within objects.
                            if (!field.DeclaringType!.IsValueType)
                                fieldAddress += factory.ObjectHeaderSize;
                            
                            // Read field value.
                            context.Machine.Memory.Read(fieldAddress, result);
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
 
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
    }
}