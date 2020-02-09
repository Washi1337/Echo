using System;
using Echo.Core.Emulation;
using Echo.DataFlow.Values;

namespace Echo.DataFlow.Emulation
{
    /// <summary>
    /// Represents a snapshot of a program during a symbolic execution.  
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that are evaluated.</typeparam>
    public class SymbolicProgramState<TInstruction> : IProgramState<SymbolicValue<TInstruction>>
    {
        /// <summary>
        /// Creates a new empty symbolic program state, setting the program counter to zero, initializing an empty
        /// stack and assigning the unknown value to all variables. 
        /// </summary>
        public SymbolicProgramState()
            : this(0, new StackState<SymbolicValue<TInstruction>>(), new VariableState<SymbolicValue<TInstruction>>(new SymbolicValue<TInstruction>()))
        {
        }

        /// <summary>
        /// Creates a new empty symbolic program state, initializing an empty stack and assigning the unknown
        /// value to all variables. 
        /// </summary>
        /// <param name="programCounter">The value of the current program counter.</param>
        public SymbolicProgramState(long programCounter)
            : this(programCounter, new StackState<SymbolicValue<TInstruction>>(), new VariableState<SymbolicValue<TInstruction>>(new SymbolicValue<TInstruction>()))
        {
        }

        /// <summary>
        /// Creates a new symbolic program state.
        /// </summary>
        /// <param name="programCounter">The value of the current program counter.</param>
        /// <param name="stack">A snapshot of the current state of the stack.</param>
        /// <param name="variables">A snapshot of the state of all variables.</param>
        public SymbolicProgramState(long programCounter, IStackState<SymbolicValue<TInstruction>> stack, IVariableState<SymbolicValue<TInstruction>> variables)
        {
            ProgramCounter = programCounter;
            Stack = stack ?? throw new ArgumentNullException(nameof(stack));
            Variables = variables ?? throw new ArgumentNullException(nameof(variables));
        }

        /// <inheritdoc />
        public long ProgramCounter
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IStackState<SymbolicValue<TInstruction>> Stack
        {
            get;
        }

        /// <inheritdoc />
        public IVariableState<SymbolicValue<TInstruction>> Variables
        {
            get;
        }

        /// <summary>
        /// Creates a deep copy of the snapshot. This includes copying the state of the stack and each variable.
        /// </summary>
        /// <returns>The copied program state.</returns>
        public SymbolicProgramState<TInstruction> Copy()
        {
            return new SymbolicProgramState<TInstruction>(ProgramCounter, Stack.Copy(), Variables.Copy());
        }

        IProgramState<SymbolicValue<TInstruction>> IProgramState<SymbolicValue<TInstruction>>.Copy() => Copy();

        /// <summary>
        /// Pulls all symbolic data sources tracked into the current snapshot. 
        /// </summary>
        /// <param name="other">The snapshot to pull data sources from.</param>
        /// <returns><c>true</c> if new data sources were introduced, <c>false</c> otherwise.</returns>
        public bool MergeWith(IProgramState<SymbolicValue<TInstruction>> other)
        {
            return Stack.MergeWith(other.Stack) | Variables.MergeWith(other.Variables);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"PC: {ProgramCounter:X8}, Stack = {{{Stack}}}";
        }
    }
}