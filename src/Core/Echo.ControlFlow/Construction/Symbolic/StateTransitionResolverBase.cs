using System;
using System.Collections.Generic;
using Echo.Core.Code;
using Echo.DataFlow;
using Echo.DataFlow.Emulation;
using Echo.DataFlow.Values;

namespace Echo.ControlFlow.Construction.Symbolic
{
    /// <summary>
    /// Provides a base implementation for a state transition resolver, that maintains a data flow graph (DFG) for
    /// resolving each program state transition an instruction might apply.  
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to evaluate.</typeparam>
    public abstract class StateTransitionResolverBase<TInstruction> : IStateTransitionResolver<TInstruction>
    {
        /// <summary>
        /// Initializes the base implementation of the state state transition resolver.
        /// </summary>
        /// <param name="architecture">The architecture that describes the instruction set.</param>
        public StateTransitionResolverBase(IInstructionSetArchitecture<TInstruction> architecture)
        {
            Architecture = architecture ?? throw new ArgumentNullException(nameof(architecture));
            DataFlowGraph = new DataFlowGraph<TInstruction>();
        }

        /// <summary>
        /// Gets the architecture for which this transition resolver is built.
        /// </summary>
        public IInstructionSetArchitecture<TInstruction> Architecture
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
        public abstract IEnumerable<StateTransition<TInstruction>> GetTransitions(SymbolicProgramState<TInstruction> currentState,
            TInstruction instruction);

        /// <summary>
        /// Applies the default fallthrough transition on a symbolic program state. 
        /// </summary>
        /// <param name="currentState">The current program state to be transitioned.</param>
        /// <param name="instruction">The instruction invoking the state transition.</param>
        protected void ApplyDefaultBehaviour(SymbolicProgramState<TInstruction> currentState, TInstruction instruction)
        {
            var node = GetOrCreateDataFlowNode(instruction);
            
            int argumentsCount = Architecture.GetStackPopCount(instruction);
            if (argumentsCount == -1)
            {
                currentState.Stack.Clear();
            }
            else
            {
                var arguments = currentState.Stack.Pop(argumentsCount, true);
                for (int i = 0; i < arguments.Count; i++)
                    node.StackDependencies[i].MergeWith(arguments[i]);
            }

            for (int i = 0; i < Architecture.GetStackPushCount(instruction); i++)
                currentState.Stack.Push(new SymbolicValue<TInstruction>(node));
            
            currentState.ProgramCounter += Architecture.GetSize(instruction);
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
                int stackArgumentCount = Architecture.GetStackPopCount(instruction);
                for (int i = 0; i < stackArgumentCount; i++)
                    node.StackDependencies.Add(new SymbolicValue<TInstruction>());
                DataFlowGraph.Nodes.Add(node);
            }

            return node;
        }
        
    }
}