
The Basics
==========

Graph-like structure form an integral part of Echo.


Graph interfaces
----------------

and are modelled in Echo using the interfaces found in the ``Echo.Core.Graphing`` namespace. The base interfaces include:

- ``IGraph``: A container for a collection of nodes and edges.
- ``INode``: A single node.
- ``IEdge``: An edge between two nodes.
- ``ISubGraph``: A collection of nodes and edges within a graph.

.. note::

    It is recommended for any package that implements some kind of graph-like structure (including trees), to use the ``INode``, ``IEdge`` and ``IGraph`` interfaces. This allows for leveraging all kinds of graph-related algorithms, such as traversals, sortings and serialization methods.

Inspecting the structure of graphs
---------------------------------- 

The contents of the graph can be accessed using the ``GetNodes()`` and ``GetEdges()`` methods:

.. code-block:: csharp

    IGraph graph = ...;

    foreach (INode node in graph.GetNodes())
        Console.WriteLine(node.Id);

    foreach (IEdge edge in graph.GetEdges())
        Console.WriteLine($"{edge.Origin.Id} -> {edge.Target.Id}");


Every node is assigned a unique `Id`. Individual nodes can be obtained from the graph using the ``GetNodeById(long)`` method. Depending on the implementation of the graph, this identifier might have different meanings. For example, in control flow graphs, the identifiers of each node is the starting offset of the basic block.

.. code-block:: csharp

    IGraph graph = ...;
    INode node = graph.GetNodeById(10);

Every node in a graph consists of at least the following methods that can be used to navigate through the graph:

- ``IEnumerable<IEdge> GetIncomingEdges()``: Gets a collection of all incoming edges i.e. edges that target this node.
- ``IEnumerable<IEdge> GetOutgoingEdges()``: Gets a collection of all outgoing edges i.e. edges originating from this node.
- ``IEnumerable<INode> GetPredecessors()``:  Gets a collection of nodes that precede this node i.e. all end points of each incoming edge.
- ``IEnumerable<INode> GetSuccessors()``: Gets a collection of nodes that succeed this node i.e. all end points of each outgoing edge.


Example implementations
-----------------------

Various parts of Echo implement the graph interfaces:

- Control Flow Graphs (CFGs)
- [Dominator trees
- Data Flow Graphs (DFGs)


Exporting Graphs
----------------

Echo also has the capability to export graphs:

- Exporting graphs to dot files