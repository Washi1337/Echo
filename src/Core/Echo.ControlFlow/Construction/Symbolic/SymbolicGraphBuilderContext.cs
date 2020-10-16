using System;
using System.Buffers;
using System.Collections.Generic;
using Echo.Core.Code;
using Echo.DataFlow.Emulation;

namespace Echo.ControlFlow.Construction.Symbolic
{
    /// <summary>
    /// Helper data structure to pass information between the transition resolver and the graph builder.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to store in the control flow graph.</typeparam>
    public sealed class SymbolicGraphBuilderContext<TInstruction> : GraphBuilderContext<TInstruction>, IDisposable
    {
        private readonly ArrayPool<StateTransition<TInstruction>> _transitionsBufferPool;
        private StateTransition<TInstruction>[] _transitionsBuffer;

        internal SymbolicGraphBuilderContext(IInstructionSetArchitecture<TInstruction> architecture)
            : base(architecture)
        {
            RecordedStates = new Dictionary<long, SymbolicProgramState<TInstruction>>();

            _transitionsBufferPool = ArrayPool<StateTransition<TInstruction>>.Shared;

            // Most common case is at most 2 transitions per instruction.
            _transitionsBuffer = _transitionsBufferPool.Rent(2);
        }

        /// <summary>
        /// Gets the map of offsets to recorded program states.
        /// </summary>
        public IDictionary<long, SymbolicProgramState<TInstruction>> RecordedStates
        {
            get;
        }

        internal StateTransition<TInstruction>[] GetTransitionsBuffer(int minimalSize)
        {
            if (_transitionsBuffer.Length < minimalSize)
            {
                _transitionsBufferPool.Return(_transitionsBuffer);
                _transitionsBuffer = _transitionsBufferPool.Rent(minimalSize);
            }

            return _transitionsBuffer;
        }

        void IDisposable.Dispose()
        {
            if (_transitionsBuffer is null)
                return;

            _transitionsBufferPool.Return(_transitionsBuffer);
            _transitionsBuffer = null;
        }
    }
}