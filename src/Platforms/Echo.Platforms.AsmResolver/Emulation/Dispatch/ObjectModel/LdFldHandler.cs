using System;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
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

            try
            {
                return field.Resolve() is { IsStatic: true }
                    ? ReadStaticField(context, field) 
                    : ReadInstanceField(context, instruction, instance);
            }
            finally
            {
                factory.BitVectorPool.Return(instance.Contents);
            }
        }
        
        private static CilDispatchResult ReadStaticField(CilExecutionContext context, IFieldDescriptor field)
        {
            // We can actually reference static fields with ldfld. The instance is then just ignored.
            
            var result = context.Machine.ValueFactory.RentValue(field.Signature!.FieldType, false);
            result.AsSpan().Write(context.Machine.StaticFields.GetFieldSpan(field));
            context.CurrentFrame.EvaluationStack.Push(result, field.Signature!.FieldType);
            
            return CilDispatchResult.Success();
        }

        private static CilDispatchResult ReadInstanceField(
            CilExecutionContext context,
            CilInstruction instruction,
            StackSlot instance)
        {
            var field = (IFieldDescriptor) instruction.Operand!;
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;
            
            var result = context.Machine.ValueFactory.RentValue(field.Signature!.FieldType, false);

            try
            {
                switch (instance.TypeHint)
                {
                    case StackSlotTypeHint.Structure:
                        // Structure was pushed onto the stack directly. Read the field directly from the structure.
                        result.AsSpan().Write(instance.Contents.AsSpan().SliceStructField(factory, field));
                        stack.Push(result, field.Signature.FieldType);
                        return CilDispatchResult.Success();

                    case StackSlotTypeHint.Float:
                        // Floats should not be able to contain a pointer to an object or a structure with fields.
                        return CilDispatchResult.InvalidProgram(context);

                    case StackSlotTypeHint.Integer:
                        // Object/structure was pushed by reference onto the stack. Dereference it.
                        var instanceSpan = instance.Contents.AsSpan();
                        long? objectAddress = instanceSpan.IsFullyKnown
                            ? instanceSpan.ReadNativeInteger(context.Machine.Is32Bit)
                            : context.Machine.UnknownResolver.ResolveSourcePointer(context, instruction, instance);

                        switch (objectAddress)
                        {
                            case null:
                                // If address is unknown even after resolution, assume it reads it from "somewhere" successfully.
                                break;

                            case 0:
                                // A null reference was passed.
                                return CilDispatchResult.NullReference(context);

                            case { } actualAddress:
                                // A non-null reference was passed.

                                var handle = field.DeclaringType!.IsValueType
                                    ? actualAddress.AsStructHandle(context.Machine)
                                    : actualAddress.AsObjectHandle(context.Machine).Contents;

                                handle.ReadField(field, result);
                                break;
                        }

                        stack.Push(result, field.Signature.FieldType);
                        return CilDispatchResult.Success();

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            finally
            {
                factory.BitVectorPool.Return(result);
            }
        }
    }
}