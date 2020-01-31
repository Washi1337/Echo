Exporting graphs to dot files
===========================

Dot files is a standard file format to store graphs in. They can also be used to visualize graphs using tools like GraphViz (Online version: http://webgraphviz.com/).

The basics
----------

To export a graph constructed by Echo to the dot file format, use the `DotWriter` class.

First make sure you have a `TextWriter` instance, such as a `StringWriter`, a `StreamWriter` or `Console.Out`:

```csharp
TextWriter writer = new StringWriter();
```

Then create a new dot writer:

```csharp
var dotWriter = new DotWriter(writer);
```

Finally, write the graph:
```csharp
IGraph graph = ...
dotWriter.Write(graph);
```

Node and edge adorners
----------------------

Nodes and edges can be decorated with additional styles. This is done through the `IDotNodeAdorner` and `IDotEdgeAdorner` interfaces, which can be passed onto an instance of a `DotWriter` class.

Echo defines a few default adorners:

- `HexLabelNodeIdentifier`: Used to put labels on the nodes in hexadecimal format.
- `ControlFlowNodeAdorner`: Used to include the contents of the embedded basic block in a node visualization.
- `ControlFlowEdgeAdorner`: Used to add colour codes to an edge based on the type of edge.