using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldflda</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ldflda)]
    public class LdFldaHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var factory = context.Machine.ValueFactory;
            
            var field = (IFieldDescriptor) instruction.Operand!;
            var instance = stack.Pop();
            var result = context.Machine.ValueFactory.RentNativeInteger(false);

            try
            {
                // We can actually reference static fields with ldfld. The instance is then just ignored.
                if (field.Resolve() is {IsStatic: true})
                    result.AsSpan().Write(context.Machine.StaticFields.GetFieldAddress(field));
                else
                    GetInstanceFieldAddress(context, instance, field, result);

                // Push.
                stack.Push(new StackSlot(result, StackSlotTypeHint.Integer));
            }
            finally
            {
                factory.BitVectorPool.Return(instance.Contents);
            }

            return CilDispatchResult.Success();
        }

        private static void GetInstanceFieldAddress(
            CilExecutionContext context, 
            StackSlot instance, 
            IFieldDescriptor field, 
            BitVector result)
        {
            if (instance.TypeHint != StackSlotTypeHint.Integer)
            {
                throw new CilEmulatorException(
                    "Attempted to get an address of an instance-field from an non-object stack value.");
            }
            
            var factory = context.Machine.ValueFactory;
            
            uint fieldOffset = factory.GetFieldMemoryLayout(field).Offset;
            
            var resultSpan = result.AsSpan();
            var fieldOffsetVector = factory.RentNativeInteger(fieldOffset);
            var objectHeaderSize = factory.RentNativeInteger(factory.ObjectHeaderSize);

            try
            {
                // Add field offset to instance pointer.
                resultSpan.Write(instance.Contents);
                resultSpan.IntegerAdd(fieldOffsetVector);

                // Skip also the object header for fields defined within objects.
                if (!field.DeclaringType!.IsValueType)
                    resultSpan.IntegerAdd(objectHeaderSize);
            }
            finally
            {
                factory.BitVectorPool.Return(fieldOffsetVector);
                factory.BitVectorPool.Return(objectHeaderSize);
            }
        }
    }
}