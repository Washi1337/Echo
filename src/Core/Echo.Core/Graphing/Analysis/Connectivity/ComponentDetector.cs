using System.Collections.Generic;
using System.Linq;

namespace Echo.Core.Graphing.Analysis.Connectivity
{
    /// <summary>
    /// Provides utility members for finding connected components within a graph.
    /// </summary>
    public static class ComponentDetector
    {
        /// <summary>
        /// Finds all strongly connected components in the provided graph.
        /// </summary>
        /// <param name="graph">The graph to get the components from.</param>
        /// <returns>A collection of sets representing the strongly connected components.</returns>
        public static ICollection<ISet<INode>> FindStronglyConnectedComponents(this IGraph graph)
        {
            // This is an implementation of the Kosaraju’s algorithm, but altered to work on disconnected graphs
            // and the transposition of the graph is replaced with looking at the predecessors rather than the
            // successors of the current node in the depth first search. 
            
            var nodes = new HashSet<INode>(graph.GetNodes());
            var visited = new HashSet<INode>();
            var result = new List<ISet<INode>>();

            while (nodes.Count != 0)
            {
                var start = nodes.First();

                var stack =  GetFillOrder(start);
                
                while (stack.Count > 0)
                {
                    var current = stack.Pop();
                    nodes.Remove(current);
                    if (!visited.Contains(current))
                        result.Add(ReverseDepthFirstSearch(visited, current));
                }
            }

            return result;
        }

        private static Stack<INode> GetFillOrder(INode start)
        {
            // This used to be a recursive function, but was recoded to be iterative to avoid stack overflows.
            
            var result = new Stack<INode>();
            
            var visited = new HashSet<INode>();
            visited.Add(start);
            
            var agenda = new Stack<FillOrderState>();
            agenda.Push(new FillOrderState(start, false));

            while (agenda.Count > 0)
            {
                var currentState = agenda.Pop();
                if (!currentState.HasTraversedSuccessors)
                {
                    visited.Add(currentState.Node);

                    // Schedule current node first to get it post-processed after the successors.
                    currentState.HasTraversedSuccessors = true;
                    agenda.Push(currentState);

                    // Schedule successors for initial processing.
                    foreach (var successor in start.GetSuccessors())
                    {
                        if (!visited.Contains(successor))
                            agenda.Push(new FillOrderState(successor, false));
                    }
                }
                else
                {
                    // current node's successors were traversed, add to the resulting stack.
                    result.Push(currentState.Node);
                }
            }

            return result;
        }

        private struct FillOrderState
        {
            public FillOrderState(INode node, bool hasTraversedSuccessors)
            {
                Node = node;
                HasTraversedSuccessors = hasTraversedSuccessors;
            }
            
            public readonly INode Node;
            public bool HasTraversedSuccessors;
        }

        private static ISet<INode> ReverseDepthFirstSearch(ISet<INode> visited, INode start)
        {
            var result = new HashSet<INode>();
            
            var agenda = new Stack<INode>();
            agenda.Push(start);

            while (agenda.Count > 0)
            {
                var current = agenda.Pop();
                result.Add(current);
                visited.Add(current);

                foreach (var predecessor in current.GetPredecessors())
                {
                    if (!visited.Contains(predecessor))
                        agenda.Push(predecessor);
                }
            }

            return result;
        }
    }
}