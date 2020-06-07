using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Core.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Provides a contract for all nodes in an AST
    /// </summary>
    public abstract class AstNodeBase : INode, IGraph
    {
        /// <summary>
        /// Assigns the node with an id
        /// </summary>
        /// <param name="id">The unique ID to assign to the node</param>
        protected AstNodeBase(long id)
        {
            Id = id;
        }
        
        /// <summary>
        /// A simple dictionary for storing user annotations
        /// </summary>
        public IDictionary<string, object> UserData
        {
            get;
        } = new Dictionary<string, object>();

        /// <inheritdoc />
        public long Id
        {
            get;
        }

        /// <inheritdoc />
        public int InDegree
        {
            get => throw new NotSupportedException();
        }

        /// <inheritdoc />
        public int OutDegree
        {
            get => GetChildren().Count();
        }

        /// <summary>
        /// Gets the children of the current AST node
        /// </summary>
        /// <returns>Immediate children of the current AST node</returns>
        public abstract IEnumerable<AstNodeBase> GetChildren();

        /// <inheritdoc />
        public IEnumerable<IEdge> GetIncomingEdges()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public IEnumerable<IEdge> GetOutgoingEdges()
        {
            return GetChildren().Select(child => new Edge(this, child)).Cast<IEdge>();
        }

        /// <inheritdoc />
        public IEnumerable<INode> GetPredecessors()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public IEnumerable<INode> GetSuccessors()
        {
            return GetChildren();
        }

        /// <inheritdoc />
        public bool HasPredecessor(INode node)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public bool HasSuccessor(INode node)
        {
            return GetChildren().Contains(node);
        }

        /// <inheritdoc />
        public INode GetNodeById(long id)
        {
            return GetNodes().Single(node => node.Id == id);
        }

        /// <inheritdoc />
        public IEnumerable<INode> GetNodes()
        {
            return GetAllChildren();
        }

        /// <inheritdoc />
        public IEnumerable<ISubGraph> GetSubGraphs()
        {
            return Array.Empty<ISubGraph>();
        }

        /// <inheritdoc />
        public IEnumerable<IEdge> GetEdges()
        {
            return GetAllChildren().Select(child => child.GetOutgoingEdges()).SelectMany(e => e);
        }

        private IEnumerable<AstNodeBase> GetAllChildren()
        {
            var children = new List<AstNodeBase>();
            
            foreach (var child in GetChildren())
                children.AddRange(child.GetAllChildren());

            return children;
        }
    }
}