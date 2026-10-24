using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Echo.Graphing;

namespace Echo.ControlFlow.Analysis.Domination
{
    /// <summary>
    /// Provides extension methods for constructing dominator trees.
    /// </summary>
    public static class DominatorTree
    {
        /// <summary>
        /// Constructs a dominator tree from a control flow graph.
        /// </summary>
        /// <param name="graph">The control flow graph to turn into a dominator tree.</param>
        /// <returns>The constructed dominator tree.</returns>
        public static DominatorTree<TInstruction> FromGraph<TInstruction>(ControlFlowGraph<TInstruction> graph)
            where TInstruction : notnull
        {
            return DominatorTree<TInstruction>.FromGraph(graph);
        }
    }

    /// <summary>
    /// Represents a dominator tree, where each tree node corresponds to one node in a graph, and each
    /// is immediately dominated by its parent.
    /// </summary>
    public class DominatorTree<TInstruction> : IGraph
        where TInstruction : notnull
    {
        private readonly IDictionary<ControlFlowNode<TInstruction>, DominatorTreeNode<TInstruction>> _nodes;
        private Dictionary<ControlFlowNode<TInstruction>, ISet<ControlFlowNode<TInstruction>>>? _frontier;
        private readonly object _frontierSyncLock = new();

        private DominatorTree(
            IDictionary<ControlFlowNode<TInstruction>, DominatorTreeNode<TInstruction>> nodes,
            ControlFlowNode<TInstruction> root)
        {
            _nodes = nodes;
            Root = nodes[root];
        }
        
        /// <summary>
        /// Gets the root of the dominator tree. That is, the tree node that corresponds to the entrypoint of the
        /// control flow graph.
        /// </summary>
        public DominatorTreeNode<TInstruction> Root
        {
            get;
        }

        /// <summary>
        /// Gets the dominator tree node associated to the given control flow graph node.
        /// </summary>
        /// <param name="node">The control flow graph node to get the tree node from.</param>
        public DominatorTreeNode<TInstruction> this[ControlFlowNode<TInstruction> node] => _nodes[node];

        /// <summary>
        /// Attempts to get the dominator tree node associated to the given control flow graph node.
        /// </summary>
        /// <param name="node">The control flow graph node to get the tree node from.</param>
        /// <param name="treeNode">The tree node, or <c>null</c> if the node is not in the tree.</param>
        /// <returns><c>True</c> if the node is in the tree, <c>false</c> otherwise.</returns>
        public bool TryGetNode(ControlFlowNode<TInstruction> node, [NotNullWhen(true)] out DominatorTreeNode<TInstruction>? treeNode) =>
            _nodes.TryGetValue(node, out treeNode);
  
        /// <summary>
        /// Constructs a dominator tree from a control flow graph.
        /// </summary>
        /// <param name="graph">The control flow graph to turn into a dominator tree.</param>
        /// <returns>The constructed dominator tree.</returns>
        public static DominatorTree<TInstruction> FromGraph(ControlFlowGraph<TInstruction> graph)
        {
            if (graph.EntryPoint == null)
                throw new ArgumentException("Control flow graph does not have an entrypoint.");
            
            return FromGraph(
                graph.EntryPoint,
                static node => node.GetSuccessors().Concat(node.GetImplicitSuccessors()));
        }

        /// <summary>
        /// Constructs a dominator tree from an entrypoint and a function that obtains the successors of a node.
        /// </summary>
        /// <param name="entrypoint">The entrypoint of the graph.</param>
        /// <param name="getSuccessors">A function that obtains the successors of a node.</param>
        /// <returns>The constructed dominator tree.</returns>
        public static DominatorTree<TInstruction> FromGraph(
            ControlFlowNode<TInstruction> entrypoint,
            Func<ControlFlowNode<TInstruction>, IEnumerable<ControlFlowNode<TInstruction>>> getSuccessors)
        {
            var traversalResult = TraverseGraph(entrypoint, getSuccessors);
            var idoms = GetImmediateDominators(entrypoint, traversalResult);
            var nodes = ConstructTreeNodes(idoms, entrypoint, traversalResult.Predecessors);
            return new DominatorTree<TInstruction>(nodes, entrypoint);
        }
        
        /// <summary>
        /// Computes the dominator tree of a control flow graph, defined by its entrypoint.
        /// </summary>
        /// <param name="entrypoint">The entrypoint of the control flow graph.</param>
        /// <param name="traversalResult">The result of traversing the graph from the entrypoint.</param>
        /// <returns>A dictionary mapping all the nodes to their immediate dominator.</returns>
        /// <remarks>
        /// The algorithm used is based on the one engineered by Lengauer and Tarjan.
        /// https://www.cs.princeton.edu/courses/archive/fall03/cs528/handouts/a%20fast%20algorithm%20for%20finding.pdf
        /// https://www.cl.cam.ac.uk/~mr10/lengtarj.pdf
        /// </remarks> 
        private static IDictionary<ControlFlowNode<TInstruction>, ControlFlowNode<TInstruction>> GetImmediateDominators(
            ControlFlowNode<TInstruction> entrypoint,
            TraversalResult traversalResult)
        {
            var immediateDominators = new Dictionary<ControlFlowNode<TInstruction>, ControlFlowNode<TInstruction>?>();
            
            var pool = ArrayPool<ControlFlowNode<TInstruction>>.Shared;
            var predecessorBuffer = pool.Rent(1);
            
            try
            {
                var semi = new Dictionary<ControlFlowNode<TInstruction>, ControlFlowNode<TInstruction>>();
                var ancestor = new Dictionary<ControlFlowNode<TInstruction>, ControlFlowNode<TInstruction>?>();
                var bucket = new Dictionary<ControlFlowNode<TInstruction>, ISet<ControlFlowNode<TInstruction>>>();

                // Initialize the intermediate mappings.
                var orderedNodes = traversalResult.TraversalOrder;
                foreach (var node in orderedNodes.Cast<ControlFlowNode<TInstruction>>())
                {
                    immediateDominators[node] = null;
                    semi[node] = node;
                    ancestor[node] = null;
                    bucket[node] = new HashSet<ControlFlowNode<TInstruction>>();
                }

                for (int i = orderedNodes.Count - 1; i >= 1; i--)
                {
                    var current = orderedNodes[i];
                    var parent = traversalResult.NodeParents[current];

                    // Grab all predecessors.
                    int predecessorCount = GetPredecessors(current);

                    // Step 2
                    for (int j = 0; j < predecessorCount; j++)
                    {
                        var u = Eval(predecessorBuffer[j], ancestor, semi, traversalResult);
                        if (traversalResult.NodeIndices[semi[current]] > traversalResult.NodeIndices[semi[u]])
                            semi[current] = semi[u];
                    }

                    bucket[semi[current]].Add(current);
                    Link(parent, current, ancestor);

                    // step 3
                    foreach (var bucketNode in bucket[parent])
                    {
                        var u = Eval(bucketNode, ancestor, semi, traversalResult);
                        if (traversalResult.NodeIndices[semi[u]] < traversalResult.NodeIndices[semi[bucketNode]])
                            immediateDominators[bucketNode] = u;
                        else
                            immediateDominators[bucketNode] = parent;
                    }

                    bucket[parent].Clear();
                }

                // step 4
                for (int i = 1; i < orderedNodes.Count; i++)
                {
                    var w = orderedNodes[i];
                    if (immediateDominators[w] != semi[w])
                        immediateDominators[w] = immediateDominators[immediateDominators[w]!];
                }

                immediateDominators[entrypoint] = entrypoint;
            }
            finally
            {
                pool.Return(predecessorBuffer);
            }

            int GetPredecessors(ControlFlowNode<TInstruction> node)
            {
                var predecessors = traversalResult.Predecessors[node];

                // Ensure we have enough space in the buffer.
                if (predecessorBuffer.Length < predecessors.Count)
                {
                    pool.Return(predecessorBuffer);
                    predecessorBuffer = pool.Rent(predecessors.Count);
                }

                predecessors.CopyTo(predecessorBuffer, 0);
                return predecessors.Count;
            }

            return immediateDominators!;
        }

        private static TraversalResult TraverseGraph(
            ControlFlowNode<TInstruction> entrypoint,
            Func<ControlFlowNode<TInstruction>, IEnumerable<ControlFlowNode<TInstruction>>> getSuccessors)
        {
            var result = new TraversalResult();
            result.TraversalOrder = new List<ControlFlowNode<TInstruction>>();
            result.NodeIndices = new Dictionary<ControlFlowNode<TInstruction>, int>();
            result.NodeParents = new Dictionary<ControlFlowNode<TInstruction>, ControlFlowNode<TInstruction>>();
            result.Predecessors = new Dictionary<ControlFlowNode<TInstruction>, HashSet<ControlFlowNode<TInstruction>>>
            {
                [entrypoint] = new HashSet<ControlFlowNode<TInstruction>>()
            };

            var visited = new HashSet<ControlFlowNode<TInstruction>>();
            var agenda = new Stack<(ControlFlowNode<TInstruction> Node, ControlFlowNode<TInstruction>? Origin)>();
            agenda.Push((entrypoint, null));

            while (agenda.Count > 0)
            {
                var (currentNode, origin) = agenda.Pop();
                if (!visited.Add(currentNode))
                    continue;

                if (origin is not null)
                    result.NodeParents[currentNode] = origin;

                result.NodeIndices[currentNode] = result.TraversalOrder.Count;
                result.TraversalOrder.Add(currentNode);

                foreach (var successor in getSuccessors(currentNode))
                    Schedule(currentNode, successor);
            }

            void Schedule(ControlFlowNode<TInstruction> origin, ControlFlowNode<TInstruction> successor)
            {
                agenda.Push((successor, origin));

                if (!result.Predecessors.TryGetValue(successor, out var predecessors))
                    result.Predecessors[successor] = predecessors = new HashSet<ControlFlowNode<TInstruction>>();
                predecessors.Add(origin);
            }

            return result;
        }

        /// <summary>
        /// Constructs a dominator tree from the control flow graph.
        /// </summary>
        /// <returns>The constructed tree. Each node added to the tree is linked to a node in the original graph by
        /// its name.</returns>
        private static IDictionary<ControlFlowNode<TInstruction>, DominatorTreeNode<TInstruction>> ConstructTreeNodes(
            IDictionary<ControlFlowNode<TInstruction>, ControlFlowNode<TInstruction>> idoms, 
            ControlFlowNode<TInstruction> entrypoint,
            IDictionary<ControlFlowNode<TInstruction>, HashSet<ControlFlowNode<TInstruction>>> predecessors)
        {
            var result = new Dictionary<ControlFlowNode<TInstruction>, DominatorTreeNode<TInstruction>>
            {
                [entrypoint] = (DominatorTreeNode<TInstruction>)new(entrypoint, predecessors[entrypoint])
            };
            
            foreach (var entry in idoms)
            {
                var dominator = entry.Value;
                var dominated = entry.Key;

                if (dominator != dominated)
                {
                    if (!result.TryGetValue(dominated, out var child))
                        result[dominated] = child = new(dominated, predecessors[dominated]);
                    if (!result.TryGetValue(dominator, out var parent))
                        result[dominator] = parent = new(dominator, predecessors[dominator]);

                    parent.Children.Add(child);
                }
            }

            return result;
        }

        private static void Link(
            ControlFlowNode<TInstruction> parent,
            ControlFlowNode<TInstruction> node,
            IDictionary<ControlFlowNode<TInstruction>, ControlFlowNode<TInstruction>?> ancestors)
        {
            ancestors[node] = parent;
        }

        private static ControlFlowNode<TInstruction> Eval(
            ControlFlowNode<TInstruction> node,
            IDictionary<ControlFlowNode<TInstruction>, ControlFlowNode<TInstruction>?> ancestors, 
            IDictionary<ControlFlowNode<TInstruction>, ControlFlowNode<TInstruction>> semi,
            in TraversalResult order)
        {
            var a = ancestors[node];
            while (a != null && ancestors[a] != null)
            {
                if (order.NodeIndices[semi[node]] > order.NodeIndices[semi[a]])
                    node = a;
                a = ancestors[a];
            }

            return node;
        }
        
        /// <summary>
        /// Determines whether one control flow graph node dominates another node. That is, whether execution of the
        /// dominated node means the dominator node has to be executed.
        /// </summary>
        /// <param name="dominator">The node that dominates.</param>
        /// <param name="dominated">The node that is potentially dominated.</param>
        /// <returns>
        /// <c>True</c> if the node in <paramref name="dominator"/> indeed dominates the provided control flow
        /// node in <paramref name="dominated"/>, <c>false</c> otherwise.
        /// </returns>
        public bool Dominates(ControlFlowNode<TInstruction> dominator, ControlFlowNode<TInstruction> dominated)
        {
            if (!_nodes.TryGetValue(dominated, out var current))
                return false;

            while (current is not null)
            {
                if (current.OriginalNode == dominator)
                    return true;
                current = current.Parent;
            }

            return false;
        }

        /// <summary>
        /// Determines the dominance frontier of a specific node. That is, the set of all nodes where the dominance of
        /// the specified node stops.
        /// </summary>
        /// <param name="node">The node to obtain the dominance frontier from.</param>
        /// <returns>A collection of nodes representing the dominance frontier.</returns>
        public IEnumerable<ControlFlowNode<TInstruction>> GetDominanceFrontier(ControlFlowNode<TInstruction> node)
        {
            if (_frontier is null)
            {
                lock (_frontierSyncLock)
                {
                    if (_frontier == null)
                        InitializeDominanceFrontiers();
                }
            }

            return _frontier[node];
        }
        
        [MemberNotNull(nameof(_frontier))]
        private void InitializeDominanceFrontiers()
        {
            var frontier = _nodes.Keys.ToDictionary(x => x, _ => (ISet<ControlFlowNode<TInstruction>>) new HashSet<ControlFlowNode<TInstruction>>());
            
            foreach (var node in _nodes.Keys)
            {
                var predecessors = _nodes[node]
                    .Predecessors
                    .ToArray();
                
                if (predecessors.Length >= 2)
                {
                    foreach (var p in predecessors)
                    {
                        var runner = p;
                        while (runner is not null && runner != _nodes[node].Parent?.OriginalNode)
                        {
                            frontier[runner].Add(node);
                            runner = _nodes[runner].Parent?.OriginalNode;
                        }
                    }
                }
            }

            _frontier = frontier;
        }

        /// <inheritdoc />
        IEnumerable<INode> ISubGraph.GetNodes() => _nodes.Values;

        /// <inheritdoc />
        public IEnumerable<ISubGraph> GetSubGraphs() => [];

        /// <inheritdoc />
        IEnumerable<IEdge> IGraph.GetEdges() => 
            _nodes.Values.SelectMany(n => n.GetOutgoingEdges());
        
        private struct TraversalResult
        {
            public Dictionary<ControlFlowNode<TInstruction>, int> NodeIndices
            {
                get;
                set;
            }

            public List<ControlFlowNode<TInstruction>> TraversalOrder
            {
                get;
                set;
            }
            
            public Dictionary<ControlFlowNode<TInstruction>, ControlFlowNode<TInstruction>> NodeParents
            {
                get;
                set;
            }

            public Dictionary<ControlFlowNode<TInstruction>, HashSet<ControlFlowNode<TInstruction>>> Predecessors
            {
                get;
                set;
            }
        } 
    }
}