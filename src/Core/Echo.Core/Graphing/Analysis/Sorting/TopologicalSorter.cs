using System.Collections.Generic;

namespace Echo.Core.Graphing.Analysis.Sorting
{
    /// <summary>
    /// Provides a mechanism for sorting nodes in a graph, such that for every edge from node A to node B in the graph we
    /// have that node A comes before node B in the final ordering, also known as a topological sorting of the graph.
    /// </summary>
    /// <typeparam name="TNode">The type of nodes to sort.</typeparam>
    public class TopologicalSorter<TNode> 
        where TNode : INode
    {
        /// <summary>
        /// Represents the method that obtains an ordered list of children of a node in a graph. 
        /// </summary>
        /// <param name="node">The node to list the children for.</param>
        public delegate IReadOnlyList<TNode> ChildrenLister(TNode node);

        /// <summary>
        /// Creates a new instance of the <see cref="TopologicalSorter{TNode}"/> class.
        /// </summary>
        /// <param name="childrenLister">The method to call when obtaining an ordered list of children of a node.</param>
        public TopologicalSorter(ChildrenLister childrenLister)
        {
            TopologicalChildrenLister = childrenLister;
            IgnoreCycles = false;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TopologicalSorter{TNode}"/> class.
        /// </summary>
        /// <param name="childrenLister">The method to call when obtaining an ordered list of children of a node.</param>
        /// <param name="ignoreCycles">Determines whether the algorithm should ignore any back-edges.</param>
        public TopologicalSorter(ChildrenLister childrenLister, bool ignoreCycles)
        {
            TopologicalChildrenLister = childrenLister;
            IgnoreCycles = ignoreCycles;
        }

        /// <summary>
        /// Gets the method to call when obtaining an ordered list of children of a node.
        /// </summary>
        public ChildrenLister TopologicalChildrenLister
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the algorithm should ignore any back-edges.
        /// </summary>
        public bool IgnoreCycles
        {
            get;
            set;
        }

        /// <summary>
        /// Obtains the topological sorting of a graph, using the provided node as the root.
        /// </summary>
        /// <param name="root">The root of the graph.</param>
        /// <returns>An ordered list of nodes, such that any node A appears before any other node B if the edge A to B
        /// exists in the graph.</returns>
        /// <exception cref="CycleDetectedException">Occurs when there was a cycle detected in the graph, and
        /// <see cref="IgnoreCycles"/> is set to <c>false</c>.</exception>
        public IEnumerable<TNode> GetTopologicalSorting(TNode root)
        {
            // We find a topological sorting of the node using the altered DFS algorithm as described here:
            // https://en.wikipedia.org/wiki/Topological_sorting
            // The algorithm used to be recursive, but was rewritten to be iterative to avoid stack overflows. 

            var result = new List<TNode>();
            var permanent = new HashSet<TNode>();
            var temporary = new HashSet<TNode>();

            var agenda = new Stack<State>();
            agenda.Push(new State(root, false));

            while (agenda.Count > 0)
            {
                var current = agenda.Pop();
                if (!current.HasTraversedDescendants)
                {
                    if (permanent.Contains(current.Node))
                        continue;
                    if (temporary.Contains(current.Node))
                        throw new CycleDetectedException();

                    temporary.Add(current.Node);

                    // Schedule remaining steps. We push this before pushing dependencies so it gets executed after
                    // the dependencies are traversed.
                    agenda.Push(new State(current.Node, true));

                    // Schedule children to be processed.
                    var children = TopologicalChildrenLister(current.Node);
                    for (int i = children.Count - 1; i >= 0; i--)
                        agenda.Push(new State(children[i], false));
                }
                else
                {
                    temporary.Remove(current.Node);
                    permanent.Add(current.Node);
                    result.Add(current.Node);
                }
            }

            return result;
        }

        private readonly struct State
        {
            public State(TNode node, bool hasTraversedDescendants)
            {
                Node = node;
                HasTraversedDescendants = hasTraversedDescendants;
            }
            
            public TNode Node
            {
                get;
            }

            public bool HasTraversedDescendants
            {
                get;
            }
        }
    }
}