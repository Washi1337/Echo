
Graphs in Echo
==============

Graph-like structure form an integral part of Echo.

How are they modelled in Echo?
------------------------------

Graphs are modelled in Echo using the interfaces found in the `Echo.Core.Graphing` namespace. The base interfaces include:
- `INode`: A single node.
- `IEdge`: An edge between two nodes.
- `IGraph`: A collection of nodes and edges.

Various parts of Echo implement these interfaces:
- [Control Flow Graphs (CFGs)](../Chapter3/README.md)
- Data Flow Graphs (DFGs)

Exporting CFGs
--------------

Echo also has the capability to export CFGs:

- [Exporting CFGs to Dot files](Dot.md)