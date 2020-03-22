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

                var stack = new Stack<INode>();
                FillOrder(new HashSet<INode>(), stack, start);
                
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

        private static void FillOrder(ISet<INode> visited, Stack<INode> stack, INode start)
        {
            visited.Add(start);
            
            foreach (var neighbour in start.GetSuccessors())
            {
                if (!visited.Contains(neighbour))
                    FillOrder(visited, stack, neighbour);
            }

            stack.Push(start);
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