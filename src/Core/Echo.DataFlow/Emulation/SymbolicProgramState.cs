using System;
using System.Collections.Immutable;
using System.Linq;
using Echo.Code;

namespace Echo.DataFlow.Emulation
{
    /// <summary>
    /// Represents an immutable snapshot of a program state that is fully symbolic.  
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions.</typeparam>
    public readonly struct SymbolicProgramState<TInstruction>
        where TInstruction : notnull
    {
        /// <summary>
        /// Gets an empty program state.
        /// </summary>
        public static SymbolicProgramState<TInstruction> Empty
        {
            get;
        } = new(0);
        
        /// <summary>
        /// Creates a new empty program state, initialized at the provided program counter.
        /// </summary>
        /// <param name="programCounter">The initial program counter.</param>
        public SymbolicProgramState(long programCounter)
        {
            ProgramCounter = programCounter;
            Stack = ImmutableStack<SymbolicValue<TInstruction>>.Empty;
            Variables = ImmutableDictionary<IVariable, SymbolicValue<TInstruction>>.Empty;
        }

        /// <summary>
        /// Creates a new empty program state, initialized at the provided program counter.
        /// </summary>
        /// <param name="programCounter">The initial program counter.</param>
        /// <param name="stack">The initial stack state.</param>
        public SymbolicProgramState(
            long programCounter, 
            ImmutableStack<SymbolicValue<TInstruction>> stack)
        {
            ProgramCounter = programCounter;
            Stack = stack ?? throw new ArgumentNullException(nameof(stack));
            Variables = ImmutableDictionary<IVariable, SymbolicValue<TInstruction>>.Empty;
        }

        /// <summary>
        /// Creates a new empty program state, initialized at the provided program counter.
        /// </summary>
        /// <param name="programCounter">The initial program counter.</param>
        /// <param name="variables">The initial state of the variables.</param>
        public SymbolicProgramState(
            long programCounter, 
            ImmutableDictionary<IVariable, SymbolicValue<TInstruction>> variables)
        {
            ProgramCounter = programCounter;
            Stack = ImmutableStack<SymbolicValue<TInstruction>>.Empty;
            Variables = variables ?? throw new ArgumentNullException(nameof(variables));
        }
        
        /// <summary>
        /// Creates a new empty program state, initialized at the provided program counter.
        /// </summary>
        /// <param name="programCounter">The initial program counter.</param>
        /// <param name="stack">The initial stack state.</param>
        /// <param name="variables">The initial state of the variables.</param>
        public SymbolicProgramState(
            long programCounter, 
            ImmutableStack<SymbolicValue<TInstruction>> stack,
            ImmutableDictionary<IVariable, SymbolicValue<TInstruction>> variables)
        {
            ProgramCounter = programCounter;
            Stack = stack ?? throw new ArgumentNullException(nameof(stack));
            Variables = variables ?? throw new ArgumentNullException(nameof(variables));
        }

        /// <summary>
        /// Gets the current value of the program counter that points to the instruction to be executed next.
        /// </summary>
        public long ProgramCounter
        {
            get;
        }

        /// <summary>
        /// Gets the current stack state of the program.
        /// </summary>
        public ImmutableStack<SymbolicValue<TInstruction>> Stack
        {
            get;
        }
        
        /// <summary>
        /// Gets the current variable state of the program.
        /// </summary>
        public ImmutableDictionary<IVariable, SymbolicValue<TInstruction>> Variables
        {
            get;
        }

        /// <summary>
        /// Copies the current state and moves the program counter of the copy to the provided address.
        /// </summary>
        /// <param name="programCounter">The new program counter.</param>
        /// <returns>The new program state.</returns>
        public SymbolicProgramState<TInstruction> WithProgramCounter(long programCounter) => 
            new(programCounter, Stack, Variables);
        
        /// <summary>
        /// Copies the current state and replaces the stack state with a new one.
        /// </summary>
        /// <param name="stack">The new stack state.</param>
        /// <returns>The new program state.</returns>
        public SymbolicProgramState<TInstruction> WithStack(ImmutableStack<SymbolicValue<TInstruction>> stack) => 
            new(ProgramCounter, stack, Variables);
        
        /// <summary>
        /// Copies the current state and replaces the variables state with a new one.
        /// </summary>
        /// <param name="variables">The new variables state.</param>
        /// <returns>The new program state.</returns>
        public SymbolicProgramState<TInstruction> WithVariables(ImmutableDictionary<IVariable, SymbolicValue<TInstruction>> variables) => 
            new(ProgramCounter, Stack, variables);

        /// <summary>
        /// Copies the current state and pushes a new value onto the stack. 
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>The new program state.</returns>
        public SymbolicProgramState<TInstruction> Push(SymbolicValue<TInstruction> value) => 
            new(ProgramCounter, Stack.Push(value), Variables);

        /// <summary>
        /// Copies the current state and pops the top value from the stack. 
        /// </summary>
        /// <param name="value">The popped value.</param>
        /// <exception cref="StackImbalanceException">Occurs when the stack is empty.</exception>
        /// <returns>The new program state.</returns>
        public SymbolicProgramState<TInstruction> Pop(out SymbolicValue<TInstruction> value)
        {
            if (Stack.IsEmpty)
                throw new StackImbalanceException(ProgramCounter);
            return new(ProgramCounter, Stack.Pop(out value), Variables);
        }

        /// <summary>
        /// Merges two program states together, combining all data sources.
        /// </summary>
        /// <param name="otherState">The other program state to merge with.</param>
        /// <param name="newState">The newly created state.</param>
        /// <returns><c>true</c> if the state has changed, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException">Occurs when the program counters do not match.</exception>
        /// <exception cref="StackImbalanceException">Occurs when the stack heights do not match.</exception>
        public bool MergeStates(in SymbolicProgramState<TInstruction> otherState, out SymbolicProgramState<TInstruction> newState)
        {
            if (ProgramCounter != otherState.ProgramCounter)
                throw new ArgumentException("Input program state has a different program counter.");

            bool changed = false;

            var newStack = otherState.Stack;
            changed |= MergeStacks(ref newStack);
            var newVariables = otherState.Variables;
            changed |= MergeVariables(ref newVariables);

            newState = new SymbolicProgramState<TInstruction>(ProgramCounter, newStack, newVariables);
            return changed;
        }

        private bool MergeStacks(ref ImmutableStack<SymbolicValue<TInstruction>> other)
        {
            var stack1 = Stack;
            var stack2 = other;
            
            // If stack heights are not the same, then we are experiencing stack imbalance,
            // which means we cannot calculate the merged state.
            int count = stack1.Count();
            if (count != stack2.Count())
                throw new StackImbalanceException(ProgramCounter);

            bool changed = false;
            
            var result = new SymbolicValue<TInstruction>[count];

            count--;
            while(count >= 0)
            {
                // Pop top values from both stacks.
                stack1 = stack1.Pop(out var value1);
                stack2 = stack2.Pop(out var value2);

                // If the two values have different data sources, we must merge them,
                // and thus create a new stack state.
                var newValue = value1;
                if (!value1.SetEquals(value2))
                {
                    newValue = new SymbolicValue<TInstruction>(value1, value2);
                    changed = true;
                }

                result[count] = newValue;
                count--;
            }

            if (changed)
                other = ImmutableStack.Create(result);

            return changed;
        }

        private bool MergeVariables(ref ImmutableDictionary<IVariable, SymbolicValue<TInstruction>> newVariables)
        {
            var result = Variables;
            bool changed = false;

            foreach (var entry in newVariables)
            {
                var variable = entry.Key;
                var otherValue = entry.Value;

                if (!result.TryGetValue(variable, out var value))
                {
                    // Variable doesn't exist in our current state yet. Reuse the item and add it to the result.
                    result = result.SetItem(variable, otherValue);
                    changed = true;
                }
                else if (!value.SetEquals(otherValue))
                {
                    // Variable does exist but has different data sources. Create new merged symbolic value.
                    var newValue = new SymbolicValue<TInstruction>(value, otherValue);
                    result = result.SetItem(variable, newValue);
                    changed = true;
                }
            }

            if (changed)
                newVariables = result;
            return changed;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(ProgramCounter)}: {ProgramCounter:X8}, {nameof(Stack)}: {Stack.Count()} slots";
        }
    }
}