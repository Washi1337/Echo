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
        /// <param name="content">Instruction to represent in the AST node</param>
        protected AstNodeBase(long id, TInstruction content)
        {
            Id = id;
            Content = content;
        }

        /// <summary>
        /// Gets the <typeparamref name="TInstruction"/> represented by this AST node
        /// </summary>
        public TInstruction Content
        {
            get;
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