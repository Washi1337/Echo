Traversal of graphs
===================

Traversing a control flow graph can be done manually, or done through a `ITraversal` object, residing in the `Echo.Core.Graphing.Analysis.Traversal` namespace.

Manual Traversal
----------------

Every graph has a collectiong of nodes of type `INode`, and every node in a graph consists of at least the following methods that can be used to navigate through the graph:

- `IEnumerable<IEdge> GetIncomingEdges()`: Gets a collection of all incoming edges i.e. edges that target this node.
- `IEnumerable<IEdge> GetOutgoingEdges()`: Gets a collection of all outgoing edges i.e. edges originating from this node.
- `IEnumerable<INode> GetPredecessors()`:  Gets a collection of nodes that precede this node i.e. all end points of each incoming edge.
- `IEnumerable<INode> GetSuccessors()`: Gets a collection of nodes that succeed this node i.e. all end points of each outgoing edge.

Built-in Traversals
-------------------

With the methods described in the above, one could write algorithms for many traversal strategies of the graph. However, Echo comes with a few built-in, stored in the `Echo.Core.Graphing.Analysis.Traversal` namespace. Either by subscribing directly to the `NodeDiscovered` event, or using a `TraversalOrderRecorder`, you can obtain the traversed nodes in the order that the algorithm prescribes.

Below an example of a depth-first traversal:

```csharp
using Echo.Core.Graphing.Analysis.Traversal;

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