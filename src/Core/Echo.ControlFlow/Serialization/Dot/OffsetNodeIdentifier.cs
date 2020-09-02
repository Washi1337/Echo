using System;
using Echo.Core.Graphing;
using Echo.Core.Graphing.Serialization.Dot;

namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Provides an implementation of the <see cref="INodeIdentifier"/> interface, that returns the offset of the basic
    /// block as unique identifiers.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the basic block.</typeparam>
    public class OffsetNodeIdentifier<TInstruction> : INodeIdentifier
    {
        /// <summary>
        /// Provides a default instance of the <see cref="OffsetNodeIdentifier{TInstruction}"/> class. 
        /// </summary>
        public static OffsetNodeIdentifier<TInstruction> Instance
        {
            get;
        } = new OffsetNodeIdentifier<TInstruction>();
        
        /// <inheritdoc />
        public long GetIdentifier(INode node) => node is ControlFlowNode<TInstruction> controlFlowNode
            ? controlFlowNode.Offset
            : throw new ArgumentOutOfRangeException(nameof(node));
    }
}