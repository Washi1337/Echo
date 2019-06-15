using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Echo.Core.Code;

namespace Echo.ControlFlow.Collections
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class NodeCollection<TInstruction> : ICollection<Node<TInstruction>> 
        where TInstruction : IInstruction
    {
        private readonly ISet<Node<TInstruction>> _nodes = new HashSet<Node<TInstruction>>();

        internal NodeCollection(Graph<TInstruction> parentGraph)
        {
            ParentGraph = parentGraph ?? throw new ArgumentNullException(nameof(parentGraph));
        }

        public Graph<TInstruction> ParentGraph
        {
            get;
        }
        
        public int Count => _nodes.Count;

        public bool IsReadOnly => false;
        
        public IEnumerator<Node<TInstruction>> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Node<TInstruction> item)
        {
            if (item.ParentGraph == ParentGraph)
                return;

            if (item.ParentGraph != null)
                throw new ArgumentException("Cannot add a node from another graph.");
            
            if (_nodes.Add(item))
                item.ParentGraph = ParentGraph;
        }

        public void AddRange(IEnumerable<Node<TInstruction>> items)
        {
            var nodes = items.ToArray();
            if (nodes.Any(n => n.ParentGraph != ParentGraph && n.ParentGraph != null))
                throw new ArgumentException("Sequence contains nodes from another graph.");

            _nodes.UnionWith(nodes);
            foreach (var node in nodes)
                node.ParentGraph = ParentGraph;
        }
        
        public void Clear()
        {
            foreach (var node in _nodes.ToArray())
                Remove(node);
        }

        public bool Contains(Node<TInstruction> item)
        {
            return _nodes.Contains(item);
        }

        public void CopyTo(Node<TInstruction>[] array, int arrayIndex)
        {
            _nodes.CopyTo(array, arrayIndex);
        }

        public bool Remove(Node<TInstruction> item)
        {
            if (_nodes.Remove(item))
            {
                item.ParentGraph = null;
                return true;
            }

            return false;
        }
        
    }
}