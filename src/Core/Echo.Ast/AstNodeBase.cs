using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Core.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for all Ast nodes
    /// </summary>
    public abstract class AstNodeBase<TInstruction> : INode
    {
        /// <summary>
        /// Assigns the unique ID to the node
        /// </summary>
        /// <param name="id">The unique identifier</param>
        protected AstNodeBase(long id)
        {
            Id = id;
        }

        /// <inheritdoc />
        public long Id
        {
            get;
        }

        /// <inheritdoc />
        public int InDegree => throw new NotSupportedException();

        /// <inheritdoc />
        public int OutDegree => GetOutgoingEdges().Count();

        /// <summary>
        /// Gets the children of the current <see cref="AstNodeBase{TInstruction}"/>
        /// </summary>
        /// <returns>The children</returns>
        public abstract IEnumerable<AstNodeBase<TInstruction>> GetChildren();

        internal virtual void Accept(VariableExpressionVisitor<TInstruction> visitor) { }

        /// <inheritdoc />
        public IEnumerable<IEdge> GetIncomingEdges() => throw new NotSupportedException();

        /// <inheritdoc />
        public IEnumerable<IEdge> GetOutgoingEdges() => 
            GetChildren().Select(child => (IEdge) new Edge(this, child));

        /// <inheritdoc />
        public IEnumerable<INode> GetPredecessors() => throw new NotSupportedException();

        /// <inheritdoc />
        public IEnumerable<INode> GetSuccessors() => GetChildren();

        /// <inheritdoc />
        public bool HasPredecessor(INode node) => throw new NotSupportedException();

        /// <inheritdoc />
        public bool HasSuccessor(INode node) => GetSuccessors().Contains(node);
    }
}