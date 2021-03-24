using System;
using System.Collections.Immutable;
using System.Linq;
using Echo.Core.Code;
using Echo.Core.Emulation;

namespace Echo.DataFlow.Emulation
{
    public readonly struct SymbolicProgramState<T>
    {
        public static SymbolicProgramState<T> Empty
        {
            get;
        } = new(0);
        
        public SymbolicProgramState(long programCounter)
        {
            ProgramCounter = programCounter;
            Stack = ImmutableStack<SymbolicValue<T>>.Empty;
            Variables = ImmutableDictionary<IVariable, SymbolicValue<T>>.Empty;
        }

        public SymbolicProgramState(
            long programCounter, 
            ImmutableStack<SymbolicValue<T>> stack)
        {
            ProgramCounter = programCounter;
            Stack = stack;
            Variables = ImmutableDictionary<IVariable, SymbolicValue<T>>.Empty;
        }

        public SymbolicProgramState(
            long programCounter, 
            ImmutableDictionary<IVariable, SymbolicValue<T>> variables)
        {
            ProgramCounter = programCounter;
            Stack = ImmutableStack<SymbolicValue<T>>.Empty;
            Variables = variables;
        }
        
        public SymbolicProgramState(
            long programCounter, 
            ImmutableStack<SymbolicValue<T>> stack,
            ImmutableDictionary<IVariable, SymbolicValue<T>> variables)
        {
            ProgramCounter = programCounter;
            Stack = stack;
            Variables = variables;
        }

        public long ProgramCounter
        {
            get;
        }
        
        public ImmutableStack<SymbolicValue<T>> Stack
        {
            get;
        }

        public ImmutableDictionary<IVariable, SymbolicValue<T>> Variables
        {
            get;
        }

        public SymbolicProgramState<T> WithProgramCounter(long programCounter) => 
            new(programCounter, Stack, Variables);
        
        public SymbolicProgramState<T> WithStack(ImmutableStack<SymbolicValue<T>> stack) => 
            new(ProgramCounter, stack, Variables);
        
        public SymbolicProgramState<T> WithVariables(ImmutableDictionary<IVariable, SymbolicValue<T>> variables) => 
            new(ProgramCounter, Stack, variables);

        public SymbolicProgramState<T> Push(SymbolicValue<T> value) => 
            new(ProgramCounter, Stack.Push(value), Variables);

        public SymbolicProgramState<T> Pop(out SymbolicValue<T> value) => 
            new(ProgramCounter, Stack.Pop(out value), Variables);

        public bool MergeStates(in SymbolicProgramState<T> otherState, out SymbolicProgramState<T> newState)
        {
            if (ProgramCounter != otherState.ProgramCounter)
                throw new ArgumentException("Input program state has a different program counter.");

            bool changed = false;

            var newStack = otherState.Stack;
            changed |= MergeStacks(ref newStack);
            var newVariables = otherState.Variables;
            changed |= MergeVariables(ref newVariables);

            newState = new SymbolicProgramState<T>(ProgramCounter, newStack, newVariables);
            return changed;
        }

        private bool MergeStacks(ref ImmutableStack<SymbolicValue<T>> other)
        {
            var stack1 = Stack;
            var stack2 = other;
            
            // If stack heights are not the same, then we are experiencing stack imbalance,
            // which means we cannot calculate the merged state.
            int count = stack1.Count();
            if (count != stack2.Count())
                throw new StackImbalanceException(ProgramCounter);

            bool changed = false;
            
            var result = new SymbolicValue<T>[count];

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
                    newValue = new SymbolicValue<T>();
                    newValue.UnionWith(value1);
                    newValue.UnionWith(value2);
                    changed = true;
                }

                result[count] = newValue;
                count--;
            }

            if (changed)
                other = ImmutableStack.Create(result);

            return changed;
        }

        private bool MergeVariables(ref ImmutableDictionary<IVariable, SymbolicValue<T>> newVariables)
        {
            var result = Variables;
            bool changed = false;

            foreach (var entry in newVariables)
            {
                var variable = entry.Key;
                var otherValue = entry.Value;

                if (!result.TryGetValue(variable, out var value))
                {
                    // Variable doesn't exist in our current state yet. Just copy the item and add it.
                    result = result.SetItem(variable, otherValue);
                    changed = true;
                }
                else if (!value.SetEquals(otherValue))
                {
                    // Variable does exist but has different data sources. Create new merged symbolic value.
                    var newValue = new SymbolicValue<T>();
                    newValue.UnionWith(value);
                    newValue.UnionWith(otherValue);
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