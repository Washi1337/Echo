using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Core.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for all Ast nodes
    /// </summary>
    public abstract class AstNodeBase : INode
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
        /// Gets the children of the current <see cref="AstNodeBase"/>
        /// </summary>
        /// <returns>The children</returns>
        public abstract IEnumerable<AstNodeBase> GetChildren();

        /// <inheritdoc />
        public IEnumerable<IEdge> GetIncomingEdges() => throw new NotSupportedException();

        /// <inheritdoc />
        public IEnumerable<IEdge> GetOutgoingEdges()
        {
            IEnumerable<Edge> edges = GetChildren().Select(child => new Edge(this, child));
            return edges.Cast<IEdge>();
        }

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