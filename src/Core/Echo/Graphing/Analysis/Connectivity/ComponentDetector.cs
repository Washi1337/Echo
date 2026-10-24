using System.Collections.Generic;

namespace Echo.Graphing.Analysis.Connectivity
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
            
            var visited = new HashSet<INode>();
            var result = new List<ISet<INode>>();

            var stack = GetFillOrder(graph.GetNodes());

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!visited.Contains(current))
                    result.Add(ReverseDepthFirstSearch(visited, current));
            }

            return result;
        }

        private static Stack<INode> GetFillOrder(IEnumerable<INode> nodes)
        {
            // This used to be a recursive function, but was recoded to be iterative to avoid stack overflows.
            
            var result = new Stack<INode>();
            
            var visited = new HashSet<INode>();
            
            var agenda = new Stack<FillOrderState>();
            foreach (var node in nodes)
                agenda.Push(new FillOrderState(node, false));

            while (agenda.Count > 0)
            {
                var currentState = agenda.Pop();
                if (!currentState.HasTraversedSuccessors)
                {
                    if (!visited.Add(currentState.Node))
                        continue;

                    // Schedule current node first to get it post-processed after the successors.
                    currentState.HasTraversedSuccessors = true;
                    agenda.Push(currentState);

                    // Schedule successors for initial processing.
                    foreach (var successor in currentState.Node.GetSuccessors())
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