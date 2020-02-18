using System.Collections.Generic;
using System.Linq;

namespace Echo.DataFlow.Analysis
{
    /// <summary>
    /// Provides members for collecting data dependencies in a data flow graph.
    /// </summary>
    public static class DependencyCollection
    {
        /// <summary>
        /// Collects all dependency nodes recursively, and sorts them in a topological order such that the final collection
        /// of nodes can be executed sequentially.
        /// </summary>
        /// <param name="node">The node to find all dependencies for.</param>
        /// <typeparam name="T">The type of contents that each node contains.</typeparam>
        /// <returns>The topological ordering of all dependencies of the node.</returns>
        /// <exception cref="CyclicDependencyException">Occurs when there is a cyclic dependency in the graph.</exception>
        public static IEnumerable<DataFlowNode<T>> GetOrderedDependencies<T>(this DataFlowNode<T> node)
        {
            // We find a topological sorting of the node using the altered DFS algorithm as described here:
            // https://en.wikipedia.org/wiki/Topological_sorting
            // The algorithm used to be recursive, but was rewritten to be iterative to avoid stack overflows. 

            var result = new List<DataFlowNode<T>>();
            var permanent = new HashSet<DataFlowNode<T>>();
            var temporary = new HashSet<DataFlowNode<T>>();

            var agenda = new Stack<State<T>>();
            agenda.Push(new State<T>(node, false));

            while (agenda.Count > 0)
            {
                var current = agenda.Pop();
                if (!current.HasTraversedDescendants)
                {
                    if (permanent.Contains(current.Node))
                        continue;
                    if (temporary.Contains(current.Node))
                        throw new CyclicDependencyException();

                    temporary.Add(current.Node);

                    // Schedule remaining steps. We push this before pushing dependencies so it gets executed after
                    // the dependencies are traversed.
                    agenda.Push(new State<T>(current.Node, true));

                    // Schedule variable dependencies.
                    foreach (var dependency in current.Node.VariableDependencies.Values)
                        agenda.Push(new State<T>(dependency.DataSources.First(), false));

                    // Schedule stack dependencies, in reversed order to ensure that the first dependency is pushed
                    // last and therefore traversed first.
                    for (int i = current.Node.StackDependencies.Count - 1; i >= 0; i--)
                        agenda.Push(new State<T>(current.Node.StackDependencies[i].DataSources.First(), false));
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

        private readonly struct State<T>
        {
            public State(DataFlowNode<T> node, bool hasTraversedDescendants)
            {
                Node = node;
                HasTraversedDescendants = hasTraversedDescendants;
            }
            
            public DataFlowNode<T> Node
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