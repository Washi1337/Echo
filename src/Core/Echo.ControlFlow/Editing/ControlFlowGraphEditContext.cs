using System;
using System.Collections.Generic;

namespace Echo.ControlFlow.Editing
{
    public class ControlFlowGraphEditContext<TInstruction> 
    {
        private readonly List<long> _nodeOffsets = new List<long>();

        public ControlFlowGraphEditContext(ControlFlowGraph<TInstruction> graph)
        {
            Graph = graph ?? throw new ArgumentNullException(nameof(graph));
            BuildNodeOffsetIndex();
        }

        public ControlFlowGraph<TInstruction> Graph
        {
            get;
        }

        public void BuildNodeOffsetIndex()
        {
            _nodeOffsets.Clear();
            _nodeOffsets.Capacity = Graph.Nodes.Count;
            foreach (var node in Graph.Nodes)
                _nodeOffsets.Add(node.Offset);
            _nodeOffsets.Sort();
        }
        
        public ControlFlowNode<TInstruction> FindNode(long offset)
        {
            // Shortcut: check if a node with the provided offset exists in the graph first.
            if (Graph.Nodes.Contains(offset))
                return Graph.Nodes[offset];

            // Find the node that contains the offset.
            return FindNodeSlow(offset, false, out _);
        }

        public ControlFlowNode<TInstruction> FindNodeOrSplit(long offset, out bool hasSplitted)
        {
            // Shortcut: check if a node with the provided offset exists in the graph first.
            if (Graph.Nodes.Contains(offset))
            {
                hasSplitted = false;
                return Graph.Nodes[offset];
            }

            // Find the node that contains the offset.
            return FindNodeSlow(offset, true, out hasSplitted);
        }

        private ControlFlowNode<TInstruction> FindNodeSlow(long offset, bool splitIfNotHeader, out bool hasSplitted)
        {
            hasSplitted = false;
            
            int index = FindClosestNodeIndex(offset);
            if (index != -1)
            {
                var node = Graph.Nodes[_nodeOffsets[index]];
                index = FindInstructionIndex(node.Contents.Instructions, offset);
                if (index != -1)
                {
                    if (index > 0 && splitIfNotHeader)
                    {
                        (node, _) = node.SplitAtIndex(index);
                        hasSplitted = true;
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