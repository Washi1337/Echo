using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Echo.Code;
using Echo.DataFlow.Emulation;

namespace Echo.DataFlow.Construction
{
    /// <summary>
    /// Provides a base implementation for a state transition resolver, that maintains a data flow graph (DFG) for
    /// resolving each program state transition an instruction might apply.  
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to evaluate.</typeparam>
    public abstract class StateTransitioner<TInstruction> : IStateTransitioner<TInstruction>
        where TInstruction : notnull
    {
        private readonly List<IVariable> _variablesBuffer = new();
        
        /// <summary>
        /// Initializes the base implementation of the state state transition resolver.
        /// </summary>
        /// <param name="architecture">The architecture that describes the instruction set.</param>
        public StateTransitioner(IArchitecture<TInstruction> architecture)
        {
            Architecture = architecture ?? throw new ArgumentNullException(nameof(architecture));
            DataFlowGraph = new DataFlowGraph<TInstruction>(architecture);
        }

        /// <summary>
        /// Gets the architecture for which this transition resolver is built.
        /// </summary>
        public IArchitecture<TInstruction> Architecture
        {
            get;
        }

        /// <summary>
        /// Gets the data flow graph that was constructed during the resolution of all transitions. 
        /// </summary>
        public DataFlowGraph<TInstruction> DataFlowGraph
        {
            get;
        }

        /// <inheritdoc />
        public virtual SymbolicProgramState<TInstruction> GetInitialState(long entrypointAddress) => new(entrypointAddress);

        /// <inheritdoc />
        public abstract void GetTransitions(
            in SymbolicProgramState<TInstruction> currentState,
            in TInstruction instruction, 
            IList<StateTransition<TInstruction>> transitionsBuffer
        );

        /// <summary>
        /// Applies the default fallthrough transition on a symbolic program state. 
        /// </summary>
        /// <param name="currentState">The current program state to be transitioned.</param>
        /// <param name="instruction">The instruction invoking the state transition.</param>
        protected SymbolicProgramState<TInstruction> ApplyDefaultBehaviour(
            in SymbolicProgramState<TInstruction> currentState,
            TInstruction instruction)
        {
            var node = GetOrCreateDataFlowNode(instruction);

            long newPc =currentState.ProgramCounter + Architecture.GetSize(instruction);
            var newStack = ApplyStackTransition(node, currentState.Stack);
            var newVariables = ApplyVariableTransition(node, currentState.Variables);

            return new SymbolicProgramState<TInstruction>(newPc, newStack, newVariables);
        }

        private ImmutableStack<SymbolicValue<TInstruction>> ApplyStackTransition(
            DataFlowNode<TInstruction> node, 
            ImmutableStack<SymbolicValue<TInstruction>> stack)
        {
            var instruction = node.Contents;
            
            int argumentsCount = Architecture.GetStackPopCount(instruction);
            if (argumentsCount == -1)
            {
                // Instruction clears the stack.
                stack = stack.Clear();
            }
            else
            {
                // Instruction as a number of arguments, we pop them in reverse order so that 
                // the resulting order is the order we would expect.
                for (int i = argumentsCount - 1; i >= 0; i--)
                {
                    // Contract expects StackImbalanceException to be thrown if not enough stack items are pushed.
                    if (stack.IsEmpty)
                        throw new StackImbalanceException(Architecture.GetOffset(instruction));

                    // Add the stack dependencies.
                    stack = stack.Pop(out var argument);
                    node.StackDependencies[i].UnionWith(argument.AsStackValue());
                }
            }

            // Check if instruction pushes any new symbolic values.
            for (int i = 0; i < Architecture.GetStackPushCount(instruction); i++)
                stack = stack.Push(SymbolicValue<TInstruction>.CreateStackValue(node, i));

            return stack;
        }

        private ImmutableDictionary<IVariable, SymbolicValue<TInstruction>> ApplyVariableTransition(
            DataFlowNode<TInstruction> node,
            ImmutableDictionary<IVariable, SymbolicValue<TInstruction>> variables)
        {
            var instruction = node.Contents;

            // Get read variables.
            _variablesBuffer.Clear();
            Architecture.GetReadVariables(instruction, _variablesBuffer);

            // Register variable dependencies.
            for (int i = 0; i < _variablesBuffer.Count; i++)
            {
                var variable = _variablesBuffer[i];
                if (variables.TryGetValue(variable, out var dataSources))
                    node.VariableDependencies[variable].UnionWith(dataSources.AsVariableValue());
            }

            // Get written variables.
            _variablesBuffer.Clear();
            Architecture.GetWrittenVariables(instruction, _variablesBuffer);

            // Apply variable changes in program state.
            for (int i = 0; i < _variablesBuffer.Count; i++)
            {
                var variable = _variablesBuffer[i];
                variables = variables.SetItem(variable, SymbolicValue<TInstruction>.CreateVariableValue(node, variable));
            }

            return variables;
        }

        /// <summary>
        /// Gets or adds a new a data flow graph node in the current data flow graph (DFG) that is linked to the
        /// provided instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <returns>The data flow graph</returns>
        protected DataFlowNode<TInstruction> GetOrCreateDataFlowNode(TInstruction instruction)
        {
            long offset = Architecture.GetOffset(instruction);
            DataFlowNode<TInstruction> node;

            if (DataFlowGraph.Nodes.Contains(offset))
            {
                node = DataFlowGraph.Nodes[offset];
            }
            else
            {
                node = new DataFlowNode<TInstruction>(offset, instruction);
                
                // Register (unknown) stack dependencies.
                int stackArgumentCount = Architecture.GetStackPopCount(instruction);
                for (int i = 0; i < stackArgumentCount; i++)
                    node.StackDependencies.Add(new StackDependency<TInstruction>());
                
                // Get read variables.
                _variablesBuffer.Clear();
                Architecture.GetReadVariables(instruction, _variablesBuffer);
                
                // Register (unknown) variable dependencies.
                for (int i = 0; i < _variablesBuffer.Count; i++)
                {
                    var variable = _variablesBuffer[i];
                    if (!node.VariableDependencies.ContainsVariable(variable))
                        node.VariableDependencies[variable] = new VariableDependency<TInstruction>(variable);
                }

                DataFlowGraph.Nodes.Add(node);
            }

            return node;
        }
        
    }
}