using System;
using System.Collections.Generic;

namespace Echo.ControlFlow.Editing
{
    /// <summary>
    /// Provides a workspace for editing a control flow graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
    public class ControlFlowGraphEditContext<TInstruction> 
    {
        private readonly List<long> _nodeOffsets = new List<long>();

        /// <summary>
        /// Creates a new instance of the <see cref="ControlFlowGraphEditContext{TInstruction}"/> class.
        /// </summary>
        /// <param name="graph">The graph to edit.</param>
        public ControlFlowGraphEditContext(ControlFlowGraph<TInstruction> graph)
        {
            Graph = graph ?? throw new ArgumentNullException(nameof(graph));
            FlushNodeOffsetIndex();
        }

        /// <summary>
        /// Gets the graph to be edited.
        /// </summary>
        public ControlFlowGraph<TInstruction> Graph
        {
            get;
        }

        /// <summary>
        /// Rebuilds the index of nodes and their offsets.
        /// </summary>
        /// <remarks>
        /// This method is supposed to be called every time a node is manually added or removed from the
        /// control flow graph.
        /// </remarks>
        public void FlushNodeOffsetIndex()
        {
            _nodeOffsets.Clear();
            _nodeOffsets.Capacity = Graph.Nodes.Count;
            foreach (var node in Graph.Nodes)
                _nodeOffsets.Add(node.Offset);
            _nodeOffsets.Sort();
        }

        /// <summary>
        /// Removes a node from the index.
        /// </summary>
        /// <param name="offset">The node offset.</param>
        /// <exception cref="ArgumentException">
        /// Occurs when the provided offset does not exist in the current node index.
        /// </exception>
        public void RemoveNodeFromIndex(long offset)
        {  
            int nodeIndex = FindClosestNodeIndex(offset);
            if (nodeIndex == -1 || _nodeOffsets[nodeIndex] != offset)
                throw new ArgumentException($"Node {offset:X8} was not indexed.");
            _nodeOffsets.RemoveAt(nodeIndex);
        }

        /// <summary>
        /// Finds the node that contains the provided instruction offset.
        /// </summary>
        /// <param name="offset">The offset of the instruction.</param>
        /// <returns>The node.</returns>
        /// <exception cref="ArgumentException">
        /// Occurs when there is no node in the graph containing an instruction with the provided offset.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method can only work properly if the node index is up-to-date. Consider calling <see cref="FlushNodeOffsetIndex"/>
        /// before using this method.
        /// </para>
        /// </remarks>
        public ControlFlowNode<TInstruction> FindNode(long offset)
        {
            // Shortcut: check if a node with the provided offset exists in the graph first.
            if (Graph.Nodes.Contains(offset))
                return Graph.Nodes[offset];

            // Find the node that contains the offset.
            return FindNodeSlow(offset, false, out _);
        }

        /// <summary>
        /// Finds the node that contains the provided instruction offset, and splits the node into two halves if the
        /// instruction is not a header of the found node.
        /// </summary>
        /// <param name="offset">The offset of the instruction.</param>
        /// <param name="hasSplit">Indicates whether the node was split up or not.</param>
        /// <returns>The node.</returns>
        /// <exception cref="ArgumentException">
        /// Occurs when there is no node in the graph containing an instruction with the provided offset.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method can only work properly if the node index is up-to-date. Make sure that <see cref="FlushNodeOffsetIndex"/>
        /// was called before using this method.
        /// </para>
        /// <para>
        /// When this method splits a node, the node index is updated automatically, and it is not needed to call
        /// <see cref="FlushNodeOffsetIndex"/> again.
        /// </para>
        /// </remarks>
        public ControlFlowNode<TInstruction> FindNodeOrSplit(long offset, out bool hasSplit)
        {
            // Shortcut: check if a node with the provided offset exists in the graph first.
            if (Graph.Nodes.Contains(offset))
            {
                hasSplit = false;
                return Graph.Nodes[offset];
            }

            // Find the node that contains the offset.
            return FindNodeSlow(offset, true, out hasSplit);
        }

        private ControlFlowNode<TInstruction> FindNodeSlow(long offset, bool splitIfNotHeader, out bool hasSplit)
        {
            hasSplit = false;
            
            int nodeIndex = FindClosestNodeIndex(offset);
            if (nodeIndex != -1)
            {
                var node = Graph.Nodes[_nodeOffsets[nodeIndex]];
                int instructionIndex = FindInstructionIndex(node.Contents.Instructions, offset);
                if (instructionIndex != -1)
                {
                    if (instructionIndex > 0 && splitIfNotHeader)
                    {
                        (_, node) = node.SplitAtIndex(instructionIndex);
                        _nodeOffsets.Insert(nodeIndex + 1, node.Offset);
                        hasSplit = true;
                    }

                    return node;
                }
            }
            
            throw new ArgumentException($"Node containing offset {offset:X8} was not found.");
        }

        private int FindClosestNodeIndex(long offset)
        {
            int min = 0;
            int max = _nodeOffsets.Count - 1;
            int mid = 0;
            
            while (min <= max)
            {
                mid = (min + max) / 2;
                if (offset < _nodeOffsets[mid])
                    max = mid - 1;
                else if (offset > _nodeOffsets[mid])
                    min = mid + 1;
                else
                    break;
            }

            if (min > max)
                return max;
            
            while (mid < _nodeOffsets.Count - 1)
            {
                if (_nodeOffsets[mid + 1] > offset)
                    return mid;
                mid++;
            }
            
            return -1;
        }

        private int FindInstructionIndex(IList<TInstruction> instructions, long offset)
        {
            var architecture = Graph.Architecture;
            
            int min = 0;
            int max = instructions.Count - 1;

            while (min <= max)
            {
                int mid = (min + max) / 2;
                long currentOffset = architecture.GetOffset(instructions[mid]);
                if (offset < currentOffset)
                    max = mid - 1;
                else if (offset > currentOffset)
                    min = mid + 1;
                else
                    return mid;
            }

            return -1;
        }
    }
}