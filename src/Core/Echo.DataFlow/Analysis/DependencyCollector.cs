using System.Collections.Generic;
using System.Linq;
using Echo.Core.Graphing.Analysis;
using Echo.Core.Graphing.Analysis.Sorting;

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
            try
            {
                var topologicalSorting = new TopologicalSorter<DataFlowNode<T>>(GetSortedOutgoingEdges);
                return topologicalSorting.GetTopologicalSorting(node);
            }
            catch (CycleDetectedException ex)
            {
                throw new CyclicDependencyException("Cyclic dependency was detected.", ex);
            }

            static IReadOnlyList<DataFlowNode<T>> GetSortedOutgoingEdges(DataFlowNode<T> node)
            {
                var result = new List<DataFlowNode<T>>();
                
                // Prioritize stack dependencies over variable dependencies.
                foreach (var dependency in node.StackDependencies)
                {
                    if (dependency.HasKnownDataSources)
                        result.Add(dependency.First());
                }
                
                foreach (var entry in node.VariableDependencies)
                {
                    if (entry.Value.HasKnownDataSources)
                        result.Add(entry.Value.First());
                }

                return result;
            }
        }
    }
}