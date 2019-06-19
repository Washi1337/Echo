using System;
using System.Collections.Generic;

namespace Echo.Core.Code
{
    /// <summary>
    /// Wraps a simple collection of instructions in a basic implementation of an <see cref="IInstructionProvider{TInstruction}"/>.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to store.</typeparam>
    public class ListInstructionProvider<TInstruction> : IInstructionProvider<TInstruction>
        where TInstruction : IInstruction
    {
        private readonly IDictionary<long, TInstruction> _instructions = new Dictionary<long, TInstruction>();

        /// <summary>
        /// Creates a new wrapper for a sequence of instructions.
        /// </summary>
        /// <param name="instructions">The instructions to put into the wrapper.</param>
        /// <exception cref="ArgumentException">Occurs when there are multiple instructions with the same offset.</exception>
        /// <exception cref="ArgumentNullException">Occurs when the provided instruction sequence is <c>null</c>.</exception>
        public ListInstructionProvider(IEnumerable<TInstruction> instructions)
        {
            if (instructions == null)
                throw new ArgumentNullException(nameof(instructions));
            
            foreach (var instruction in instructions)
            {
                if (_instructions.ContainsKey(instruction.Offset))
                    throw new ArgumentException($"Sequence contains multiple instructions with the offset {instruction.Offset:X8}.");
                _instructions[instruction.Offset] = instruction;
            }
        }

        /// <inheritdoc />
        public TInstruction GetInstructionAtOffset(long offset)
        {
            return _instructions[offset];
        }
        
    }
}