Traversal of Control Flow Graphs
================================

Traversing a control flow graph can be done manually, or done through a `ITraversal` object, residing in the `Echo.ControlFlow.Analysis.Traversal` namespace.

Manual Traversal
----------------

Every CFG has an **Entrypoint** and every node in a CFG consists of at least the following methods that can be used to navigate through the CFG:

- `IEdge GetFallThroughEdge()`: Gets or sets the edge to the neighbour to which the control is transferred to after execution of this block and no other condition is met.
- `IEnumerable<IEdge> GetConditionalEdges()`:  Gets a collection of conditional edges that originate from this source.
- `IEnumerable<IEdge> GetAbnormalEdges()`: Gets a collection of abnormal edges that originate from this source (typically exception handler edges).
- `IEnumerable<IEdge> GetIncomingEdges()`: Gets a collection of all edges that target this node.
- `IEnumerable<IEdge> GetOutgoingEdges()`: Gets a collection of all outgoing edges originating from this node.
- `IEnumerable<INode> GetPredecessors()`:  Gets a collection of nodes that precede this node. This includes any node that might transfer control to node this node in the complete control flow graph, regardless of edge type. 
- `IEnumerable<INode> GetSuccessors()`: Gets a collection of nodes that might be executed after this node. This includes any node that this node.

If you are using the `Node<TInstruction>` classes, you can also use the mutable properties instead.

Built-in Traversals
-------------------

With the methods described in the above, one could write algorithms for many traversal strategies of the CFG. However, Echo comes with a few built-in, stored in the `Echo.ControlFlow.Analysis.Traversal` namespace. Either by subscribing directly to the `NodeDiscovered` event, or using a `TraversalOrderRecorder`, you can obtain the traversed nodes in the order that the algorithm prescribes.

Below an example of a depth-first traversal:

```csharp
using Echo.ControlFlow.Analysis.Traversal;

// ...

// Initialize the DFS traversal:
var traversal = new DepthFirstTraversal();
var recorder = new TraversalOrderRecorder(traversal);

// Run it!
traversal.Run(graph.Entrypoint);

// Get the recorded traversal:
foreach (var node in recorder.GetTraversal())
{
    // ...
}
```

To get the index number of a specific node according to a traversal algorithm, use `TraversalOrderRecorder.GetIndex(INode)` over a normal `IList<INode>.IndexOf` call. A normal IndexOf call will perform another linear search which is O(n), whereas the GetIndex method makes use of the recorded order and is close to O(1) in lookup time:

```csharp
// Do:
int index = recorder.GetIndex(someNode);

// Don't:
int index = recorder.GetTraversal().IndexOf(someNode);
```