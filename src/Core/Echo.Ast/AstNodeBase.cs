using System.Collections.Generic;
using Echo.Core.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Provides a contract for all nodes in an AST
    /// </summary>
    public abstract class AstNodeBase : INode
    {
        /// <summary>
        /// A simple dictionary for storing annotations
        /// </summary>
        public abstract IDictionary<string, object> UserData
        {
            get;
        }

        /// <summary>
        /// Gets the children of the current AST node
        /// </summary>
        /// <returns>All children of the current AST node</returns>
        public abstract IEnumerable<AstNodeBase> GetChildren();

        /// <inheritdoc />
        public long Id
        {
            get;
        }

        /// <inheritdoc />
        public int InDegree
        {
            get;
        }

        /// <inheritdoc />
        public int OutDegree
        {
            get;
        }

        /// <inheritdoc />
        public IEnumerable<IEdge> GetIncomingEdges()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<IEdge> GetOutgoingEdges()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<INode> GetPredecessors()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<INode> GetSuccessors()
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public bool HasPredecessor(INode node)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public bool HasSuccessor(INode node)
        {
            throw new System.NotImplementedException();
        }
    }
}