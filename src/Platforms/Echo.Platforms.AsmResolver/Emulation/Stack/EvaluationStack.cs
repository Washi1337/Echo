using AsmResolver.DotNet.Signatures.Types;
using Echo.Memory;

namespace Echo.Platforms.AsmResolver.Emulation.Stack
{
    /// <summary>
    /// Represents an evaluation stack during the execution of a managed method body.
    /// </summary>
    public class EvaluationStack : IndexableStack<StackSlot>
    {
        /// <summary>
        /// Creates a new evaluation stack.
        /// </summary>
        /// <param name="factory">The value factory to use.</param>
        public EvaluationStack(ValueFactory factory)
        {
            Factory = factory;
        }

        internal ValueFactory Factory
        {
            get;
        }

        /// <summary>
        /// Puts the address of the provided object handle into a pointer-sized bit vector, and pushes it onto the stack.
        /// </summary>
        /// <param name="value">The handle to push.</param>
        public void Push(ObjectHandle value)
        {
            var vector = Factory.RentNativeInteger(value.Address);
            Push(vector, Factory.ContextModule.CorLibTypeFactory.Object, true);
        }

        /// <summary>
        /// Marshals the provided bitvector into a stack slot, and pushes it onto the stack.
        /// </summary>
        /// <param name="value">The value to push.</param>
        /// <param name="originalType">The type of the value to push.</param>
        /// <returns>The stack slot that was created.</returns>
        public void Push(BitVectorSpan value, TypeSignature originalType)
        {
            var vector = Factory.BitVectorPool.Rent(value.Count, false);
            vector.AsSpan().Write(value);
            Push(vector, originalType, true);
        }

        /// <summary>
        /// Marshals the provided bitvector into a stack slot, and pushes it onto the stack.
        /// </summary>
        /// <param name="value">The value to push.</param>
        /// <param name="originalType">The type of the value to push.</param>
        /// <returns>The stack slot that was created.</returns>
        public void Push(BitVector value, TypeSignature originalType) => Push(value, originalType, false);

        /// <summary>
        /// Marshals the provided bitvector into a stack slot, and pushes it onto the stack.
        /// </summary>
        /// <param name="value">The value to push.</param>
        /// <param name="originalType">The type of the value to push.</param>
        /// <param name="releaseBitVector">
        /// <c>true</c> if <paramref name="value"/> should be returned to the bit vector pool, <c>false</c> if the caller
        /// should remain the owner of the bit vector.
        /// </param>
        /// <returns>The stack slot that was created.</returns>
        public void Push(BitVector value, TypeSignature originalType, bool releaseBitVector)
        {
            var marshalled = Factory.Marshaller.ToCliValue(value, originalType);
            
            if (releaseBitVector)
                Factory.BitVectorPool.Return(value);
            
            Push(marshalled);
        }

        /// <summary>
        /// Pops the top-most value from the stack, and reinterprets it according to the provided target type.
        /// </summary>
        /// <param name="targetType">The type of the value to pop.</param>
        /// <returns>The popped and marshalled value.</returns>
        public BitVector Pop(TypeSignature targetType)
        {
            var slot = Pop();
            
            var marshalled = Factory.Marshaller.FromCliValue(slot, targetType);
            Factory.BitVectorPool.Return(slot.Contents);
            
            return marshalled;
        }

        /// <inheritdoc />
        public override void Clear()
        {
            for (int i = 0; i < Count; i++)
                Factory.BitVectorPool.Return(this[i].Contents);

            base.Clear();
        }
    }
}