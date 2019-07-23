using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Analysis.Traversal;

namespace Echo.ControlFlow.Analysis.Connectivity
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
            var traversal = new DepthFirstTraversal();
            var recorder = new PostOrderRecorder(traversal);

            traversal.Run(graph.Entrypoint);

            var visited = new HashSet<INode>();
            var result = new List<ISet<INode>>();
            foreach (var node in recorder.GetOrder().Reverse())
            {
                if (!visited.Contains(node))
                {
                    var subTraversal = new DepthFirstTraversal(true);
                    var component = new HashSet<INode>();
                    subTraversal.NodeDiscovered += (sender, args) =>
                    {
                        if (visited.Add(args.NewNode))
                        {
                            args.ContinueExploring = true;
                            component.Add(args.NewNode);
                        }
                    };
                    subTraversal.Run(node);

                    result.Add(component);
                }
            }

            return result;
        }       
    }
}