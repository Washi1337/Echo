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
        } = new SymbolicProgramState<T>(0);
        
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
            
            int count = stack1.Count();
            if (count != stack2.Count())
                throw new StackImbalanceException(ProgramCounter);

            bool changed = false;
            
            var result = new SymbolicValue<T>[count];

            count--;
            while(count >= 0)
            {
                stack1 = stack1.Pop(out var value1);
                stack2 = stack2.Pop(out var value2);

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
            return false;
        }

    }
}