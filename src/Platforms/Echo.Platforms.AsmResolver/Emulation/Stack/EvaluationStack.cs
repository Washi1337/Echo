using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete;
using Echo.Core;

namespace Echo.Platforms.AsmResolver.Emulation.Stack
{
    /// <summary>
    /// Represents an evaluation stack during the execution of a managed method body.
    /// </summary>
    public class EvaluationStack : IndexableStack<StackSlot>
    {
        private readonly ValueFactory _factory;

        /// <summary>
        /// Creates a new evaluation stack.
        /// </summary>
        /// <param name="factory">The value factory to use.</param>
        public EvaluationStack(ValueFactory factory)
        {
            _factory = factory;
        }

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
        public StackSlot Push(BitVector value, TypeSignature originalType, bool releaseBitVector = true)
        {
            var marshalled = _factory.Marshaller.ToCliValue(value, originalType);
            
            if (releaseBitVector)
                _factory.BitVectorPool.Return(value);
            
            Push(marshalled);

            return marshalled;
        }

        /// <summary>
        /// Pops the top-most value from the stack, and reinterprets it according to the provided target type.
        /// </summary>
        /// <param name="targetType">The type of the value to pop.</param>
        /// <returns>The popped and marshalled value.</returns>
        public BitVector Pop(TypeSignature targetType)
        {
            var slot = Pop();
            
            var marshalled = _factory.Marshaller.FromCliValue(slot, targetType);
            _factory.BitVectorPool.Return(slot.Contents);
            
            return marshalled;
        }
    }
}