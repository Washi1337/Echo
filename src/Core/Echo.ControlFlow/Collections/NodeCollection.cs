using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Echo.ControlFlow.Collections
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class NodeCollection<TContents> : ICollection<Node<TContents>>
    {
        private readonly ISet<Node<TContents>> _nodes = new HashSet<Node<TContents>>();

        internal NodeCollection(Graph<TContents> parentGraph)
        {
            ParentGraph = parentGraph ?? throw new ArgumentNullException(nameof(parentGraph));
        }

        public Graph<TContents> ParentGraph
        {
            get;
        }
        
        public int Count => _nodes.Count;

        public bool IsReadOnly => false;
        
        public IEnumerator<Node<TContents>> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Node<TContents> item)
        {
            if (item.ParentGraph == ParentGraph)
                return;

            if (item.ParentGraph != null)
                throw new ArgumentException("Cannot add a node from another graph.");
            
            if (_nodes.Add(item))
                item.ParentGraph = ParentGraph;
        }

        public void AddRange(IEnumerable<Node<TContents>> items)
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

        public bool Contains(Node<TContents> item)
        {
            return _nodes.Contains(item);
        }

        public void CopyTo(Node<TContents>[] array, int arrayIndex)
        {
            _nodes.CopyTo(array, arrayIndex);
        }

        public bool Remove(Node<TContents> item)
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