using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Collections;

namespace Echo.ControlFlow.Editing
{
    public class UpdateAdjacencyListAction<TInstruction> : IControlFlowGraphEditAction<TInstruction>
    {
        private bool _isApplied;
        private IList<long> _oldSuccessors = null;
        private BitArray _splitted = null;

        public UpdateAdjacencyListAction(long branchOffset, ControlFlowEdgeType edgeType, IEnumerable<long> newSuccessors)
        {
            BranchOffset = branchOffset;
            EdgeType = edgeType;
            NewSuccessors = new List<long>(newSuccessors);
        }
        
        public long BranchOffset
        {
            get;
        }

        public ControlFlowEdgeType EdgeType
        {
            get;
        }

        public IList<long> NewSuccessors
        {
            get;
        }
        
        public void Apply(ControlFlowGraphEditContext<TInstruction> context)
        {
            if (_isApplied)
                throw new InvalidOperationException("Operation was already applied.");

            var node = context.FindNode(BranchOffset);
            var edges = GetAdjacencyCollection(node);
            
            // Store old successors.
            _oldSuccessors = edges
                .Select(e => e.Target.Offset)
                .ToArray();
            
            edges.Clear();
            
            _splitted = new BitArray(NewSuccessors.Count);
            for (int i = 0; i < NewSuccessors.Count; i++)
            {
                long successorOffset = NewSuccessors[i];
                var successorNode = context.FindNodeOrSplit(successorOffset, out bool hasSplitted);
                _splitted[i] = hasSplitted;
                edges.Add(successorNode);
            }

            _isApplied = true;
        }

        public void Revert(ControlFlowGraphEditContext<TInstruction> context)
        {
            if (!_isApplied)
                throw new InvalidOperationException("Operation is not applied yet.");

            var node = context.FindNode(BranchOffset);
            var edges = GetAdjacencyCollection(node);
            
            edges.Clear();
            
            // Important: restore in reversed order.
            // Successors might contain duplicates, and the first duplicate might have split the node.
            for (int i = NewSuccessors.Count - 1; i >= 0; i--)
            {
                if (_splitted[i])
                {
                    long successorOffset = NewSuccessors[i];
                    var successorNode = context.FindNode(successorOffset);
                    successorNode.MergeWithPredecessor();
                }
            }

            foreach (long oldSuccessorOffset in _oldSuccessors)
                edges.Add(oldSuccessorOffset);
            
            _isApplied = false;
        }

        private AdjacencyCollection<TInstruction> GetAdjacencyCollection(ControlFlowNode<TInstruction> node)
        {
            return EdgeType switch
            {
                ControlFlowEdgeType.Conditional => node.ConditionalEdges,
                ControlFlowEdgeType.Abnormal => node.AbnormalEdges,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}