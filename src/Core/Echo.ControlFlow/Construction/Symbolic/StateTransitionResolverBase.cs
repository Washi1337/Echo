using System;
using System.Buffers;
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
        private IVariable[] _variablesBuffer = new IVariable[1];
        
        /// <summary>
        /// Initializes the base implementation of the state state transition resolver.
        /// </summary>
        /// <param name="architecture">The architecture that describes the instruction set.</param>
        public StateTransitionResolverBase(IInstructionSetArchitecture<TInstruction> architecture)
        {
            Architecture = architecture ?? throw new ArgumentNullException(nameof(architecture));
            DataFlowGraph = new DataFlowGraph<TInstruction>(architecture);
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
        public virtual SymbolicProgramState<TInstruction> GetInitialState(long entrypointAddress) => 
            new SymbolicProgramState<TInstruction>(entrypointAddress);

        /// <inheritdoc />
        public abstract int GetTransitionCount(SymbolicProgramState<TInstruction> currentState,
            in TInstruction instruction);

        /// <inheritdoc />
        public abstract int GetTransitions(SymbolicProgramState<TInstruction> currentState,
            in TInstruction instruction,
            Span<StateTransition<TInstruction>> transitionBuffer,
            Action<SymbolicProgramState<TInstruction>> addHeaderFunc);

        /// <summary>
        /// Applies the default fallthrough transition on a symbolic program state. 
        /// </summary>
        /// <param name="currentState">The current program state to be transitioned.</param>
        /// <param name="instruction">The instruction invoking the state transition.</param>
        protected void ApplyDefaultBehaviour(SymbolicProgramState<TInstruction> currentState, TInstruction instruction)
        {
            var node = GetOrCreateDataFlowNode(instruction);
            
            ApplyStackTransition(node, currentState);
            ApplyVariableTransition(node, currentState);

            currentState.ProgramCounter += Architecture.GetSize(instruction);
        }

        private void ApplyStackTransition(DataFlowNode<TInstruction> node, SymbolicProgramState<TInstruction> currentState)
        {
            var instruction = node.Contents;
            
            int argumentsCount = Architecture.GetStackPopCount(instruction);
            if (argumentsCount == -1)
            {
                currentState.Stack.Clear();
            }
            else
            {
                var arguments = currentState.Stack.Pop(argumentsCount, true);
                for (int i = 0; i < arguments.Count; i++)
                    node.StackDependencies[i].UnionWith(arguments[i]);
            }

            for (int i = 0; i < Architecture.GetStackPushCount(instruction); i++)
                currentState.Stack.Push(new SymbolicValue<TInstruction>(new DataSource<TInstruction>(node, i)));
        }

        private void ApplyVariableTransition(DataFlowNode<TInstruction> node, SymbolicProgramState<TInstruction> currentState)
        {
            var instruction = node.Contents;

            // Ensure buffer is large enough.
            int readCount = Architecture.GetReadVariablesCount(instruction);
            int writtenCount = Architecture.GetWrittenVariablesCount(instruction);
            
            int bufferSize = Math.Max(readCount, writtenCount);
            if (_variablesBuffer.Length < bufferSize)
                _variablesBuffer = new IVariable[bufferSize];
            
            // Get read variables.
            var variablesBufferSlice = new Span<IVariable>(_variablesBuffer, 0, readCount);
            int actualCount = Architecture.GetReadVariables(instruction, variablesBufferSlice);
            if (actualCount > variablesBufferSlice.Length)
                throw new ArgumentException("GetReadVariables returned a number of variables that is inconsistent.");

            // Register variable dependencies.
            for (int i = 0; i < actualCount; i++)
            {
                var variable = _variablesBuffer[i];
                node.VariableDependencies[variable].UnionWith(currentState.Variables[variable]);
            }
            
            // Get written variables.
            variablesBufferSlice = new Span<IVariable>(_variablesBuffer, 0, writtenCount);
            actualCount = Architecture.GetWrittenVariables(instruction, variablesBufferSlice);
            if (actualCount > bufferSize)
                throw new ArgumentException("GetWrittenVariables returned a number of variables that is inconsistent.");

            // Apply variable changes in program state.
            for (int i = 0; i < actualCount; i++)
            {
                var variable = _variablesBuffer[i];
                currentState.Variables[variable] = new SymbolicValue<TInstruction>(new DataSource<TInstruction>(node));
            }
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
                    node.StackDependencies.Add(new DataDependency<TInstruction>());
                
                // Get read variables.
                int variableReadCount = Architecture.GetReadVariablesCount(instruction);
                if (_variablesBuffer.Length < variableReadCount)
                    _variablesBuffer = new IVariable[variableReadCount];
                
                int actualCount = Architecture.GetReadVariables(instruction, _variablesBuffer);
                if (actualCount > variableReadCount)
                    throw new ArgumentException("GetWrittenVariables returned a number of variables that is inconsistent.");

                // Register (unknown) variable dependencies.
                for (int i = 0; i < actualCount; i++)
                {
                    var variable = _variablesBuffer[i];
                    if (!node.VariableDependencies.ContainsKey(variable))
                        node.VariableDependencies[variable] = new DataDependency<TInstruction>();
                }

                DataFlowGraph.Nodes.Add(node);
            }

            return node;
        }
        
    }
}