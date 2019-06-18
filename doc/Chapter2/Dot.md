Exporting CFGs to Dot files
===========================

Dot files is a standard file format to store graphs in. They can also be used to visualize graphs using tools like GraphViz (Online version: http://webgraphviz.com/).

To export a CFG constructed by Echo to the dot file format, use either the `DotWriter` or the `BasicBlockDotWriter` classes from the ` Echo.ControlFlow.Serialization.Dot` namespace. The difference between the two is that `DotWriter` only writes the nodes and labels them, whereas `BasicBlockDotWriter` also includes the instructions for each node.

First make sure you have a `TextWriter` instance, such as a `StringWriter`, a `StreamWriter` or `Console.Out`:

```csharp
TextWriter writer = new StringWriter();
```

Then create a new dot writer:

```csharp
var dotWriter = new DotWriter(writer); // For simple graphs
var dotWriter = new BasicBlockDotWriter<TInstruction>(writer); // To include the contents for each node.
```

Finally, write the graph:
```csharp
dotWriter.Write(graph);
```