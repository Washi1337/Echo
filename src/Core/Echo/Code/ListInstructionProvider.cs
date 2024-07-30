using System;
using System.Collections.Generic;

namespace Echo.Code
{
    /// <summary>
    /// Wraps a simple collection of instructions in a basic implementation of an <see cref="IStaticInstructionProvider{TInstruction}"/>.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to store.</typeparam>
    public class ListInstructionProvider<TInstruction> : IStaticInstructionProvider<TInstruction>
        where TInstruction : notnull
    {
        private readonly IDictionary<long, TInstruction> _instructions = new Dictionary<long, TInstruction>();

        /// <summary>
        /// Creates a new wrapper for a sequence of instructions.
        /// </summary>
        /// <param name="architecture">The instruction architecture.</param>
        /// <param name="instructions">The instructions to put into the wrapper.</param>
        /// <exception cref="ArgumentException">Occurs when there are multiple instructions with the same offset.</exception>
        /// <exception cref="ArgumentNullException">Occurs when the provided instruction sequence is <c>null</c>.</exception>
        public ListInstructionProvider(IArchitecture<TInstruction> architecture, IEnumerable<TInstruction> instructions)
        {
            if (instructions == null)
                throw new ArgumentNullException(nameof(instructions));
            Architecture = architecture;

            foreach (var instruction in instructions)
            {
                long offset = architecture.GetOffset(instruction);
                if (_instructions.ContainsKey(offset))
                    throw new ArgumentException($"Sequence contains multiple instructions with the offset {offset:X8}.");
                _instructions[offset] = instruction;
            }
        }

        /// <inheritdoc />
        public IArchitecture<TInstruction> Architecture
        {
            get;
        }

        /// <inheritdoc />
        public TInstruction GetInstructionAtOffset(long offset) => _instructions[offset];
    }
}