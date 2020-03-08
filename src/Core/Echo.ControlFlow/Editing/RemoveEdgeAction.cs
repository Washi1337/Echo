using System;
using System.Linq;

namespace Echo.ControlFlow.Editing
{
    /// <summary>
    /// Represents an action that edits a control flow graph by removing an edge from one node to another.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
    public class RemoveEdgeAction<TInstruction> : UpdateAdjacencyAction<TInstruction>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="RemoveEdgeAction{TInstruction}"/> class.
        /// </summary>
        /// <param name="originOffset">The offset to the branching instruction that is the origin of the edge to remove.</param>
        /// <param name="targetOffset">The offset to the neighbour that the edge to remove targets.</param>
        /// <param name="edgeType">The type of edge.</param>
        /// <exception cref="NotSupportedException">
        /// Occurs when <paramref name="edgeType"/> equals <see cref="ControlFlowEdgeType.FallThrough"/>
        /// </exception>
        public RemoveEdgeAction(long originOffset, long targetOffset, ControlFlowEdgeType edgeType)
            : base(originOffset, targetOffset, edgeType)
        {
            if (edgeType == ControlFlowEdgeType.FallThrough)
                throw new NotSupportedException("Fallthrough edges are not supported.");
        }

        /// <inheritdoc />
        protected override void OnApply(ControlFlowGraphEditContext<TInstruction> context)
        {
            var origin = context.FindNode(OriginOffset);
            var target = context.Graph.Nodes[TargetOffset];

            var collection = EdgeType switch
            {
                ControlFlowEdgeType.Conditional => origin.ConditionalEdges,
                ControlFlowEdgeType.Abnormal => origin.AbnormalEdges,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            collection.Remove(collection.GetEdgesToNeighbour(target).First());

            var incomingEdges = target.GetIncomingEdges().ToArray();
            if (incomingEdges.Length == 1 && incomingEdges[0].Type == ControlFlowEdgeType.FallThrough)
                target.MergeWithPredecessor();
        }

        /// <inheritdoc />
        protected override void OnRevert(ControlFlowGraphEditContext<TInstruction> context)
        {
            var origin = context.FindNode(OriginOffset);
            var target = context.FindNodeOrSplit(TargetOffset, out _);
            origin.ConnectWith(target, EdgeType);
        }

        /// <inheritdoc />
        public override string ToString() => 
            $"Remove {EdgeType} edge from {OriginOffset:X8} to {TargetOffset:X8}.";
    }
}