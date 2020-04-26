using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Regions.Detection;
using Echo.Core.Code;

namespace Echo.ControlFlow.Construction
{
    /// <summary>
    /// Provides a base for control flow graph builders that depend on a single traversal of the instructions.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to store in the control flow graph.</typeparam>
    public abstract class FlowGraphBuilderBase<TInstruction> : IFlowGraphBuilder<TInstruction>
    {
        /// <inheritdoc />
        public abstract IInstructionSetArchitecture<TInstruction> Architecture
        {
            get;
        }
        
        /// <inheritdoc />
        public ControlFlowGraph<TInstruction> ConstructFlowGraph(long entrypoint, IEnumerable<long> knownBlockHeaders)
        {
            var traversalResult = CollectInstructions(entrypoint, knownBlockHeaders);

            var graph = new ControlFlowGraph<TInstruction>(Architecture);
            CreateNodes(graph, traversalResult);
            ConnectNodes(graph, traversalResult);
            graph.Entrypoint = graph.GetNodeByOffset(entrypoint);
            
            return graph;
        }

        /// <summary>
        /// Traverses the instructions and records block headers and successor information about each traversed instruction.
        /// </summary>
        /// <param name="entrypoint">The address of the first instruction to traverse.</param>
        /// <param name="knownBlockHeaders">A list of known block headers that should be included in the traversal.</param>
        /// <returns>An object containing the result of the traversal, including the block headers and successors of
        /// each instruction.</returns>
        protected abstract IInstructionTraversalResult<TInstruction> CollectInstructions(
            long entrypoint, IEnumerable<long> knownBlockHeaders);

        private void CreateNodes(ControlFlowGraph<TInstruction> graph, IInstructionTraversalResult<TInstruction> traversalResult)
        {
            ControlFlowNode<TInstruction> currentNode = null;
            foreach (var info in traversalResult.GetAllInstructions())
            {
                // Check if we reached a new block header.
                long offset = Architecture.GetOffset(info.Instruction);
                if (currentNode == null || traversalResult.IsBlockHeader(offset))
                {
                    // We arrived at a new basic block header. Create a new node for it. 
                    currentNode = new ControlFlowNode<TInstruction>(offset);
                    graph.Nodes.Add(currentNode);
                }

                // Add the current instruction to the current block that we're populating.
                currentNode.Contents.Instructions.Add(info.Instruction);

                // Edge case: Blocks might not come straight after each other. If we see that the next instruction
                // does not exist, or the previous algorithm has decided the next instruction is a block header, 
                // we can create a new block in the next iteration.
                long nextOffset = offset + Architecture.GetSize(info.Instruction);
                if (!traversalResult.ContainsInstruction(nextOffset) || traversalResult.IsBlockHeader(nextOffset))
                    currentNode = null;
            }
        }

        private void ConnectNodes(ControlFlowGraph<TInstruction> graph, IInstructionTraversalResult<TInstruction> traversalResult)
        {
            foreach (var node in graph.Nodes)
            {
                // Get the successors of the last instruction in the current block.
                var block = node.Contents;
                long footerOffset = Architecture.GetOffset(block.Instructions[block.Instructions.Count - 1]);
                var successors = traversalResult.GetInstruction(footerOffset).Successors;

                // Add edges accordingly.
                foreach (var successor in successors)
                {
                    var successorNode = graph.GetNodeByOffset(successor.DestinationAddress);
                    if (successorNode == null)
                    {
                        throw new ArgumentException(
                            $"Instruction at address {footerOffset:X8} refers to a non-existing node.");
                    }

                    node.ConnectWith(successorNode, successor.EdgeType);
                }
            }
        }
        
    }
}