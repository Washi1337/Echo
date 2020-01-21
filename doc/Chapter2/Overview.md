Control Flow Analysis
=====================

What are Control Flow Graphs?
-----------------------------

A control flow graph (CFG) is a graph where each node corresponds to one basic block of code, and each edge represents a possible control flow transfer. This way, a control flow graph tries to encode all possible execution paths in a chunk of code, method or entire program.

The figure below depicts a control flow graph of a very simple if-statement:

![If statement](img/if.png)

How are they modelled in Echo?
------------------------------

Graphs are modelled in Echo using the classes found in the `Echo.ControlFlow` namespace. The base interfaces include:
- `INode`: A single node.
- `IEdge`: An edge between two nodes.
- `IGraph`: A collection of nodes and edges, with one node selected as the entrypoint.

A default implementation is given to represent nodes with some generic data type stored in each of them. Given a type `TContents`, one could use the following classes to model a CFG:
- `Node<TContents>`
- `Edge<TContents>`
- `Graph<TContents>`

However, when specifically dealing with CFGs, you probably will only deal with the following classes instead:
- `ControlFlowGraph<TInstruction>`
- `BasicBlockNode<TInstruction>` 
- `BasicBlock<TInstruction>`

These are convenience classes defined in the `Echo.ControlFlow.Specialized` interface that simplify your expressions a bit.

Constructing CFGs
-----------------

Echo supports constructing control flow graphs from chunks of code for a variety of platforms, provided that these platforms implement just a couple of interfaces.

- [Static CFG construction](StaticCfg.md)

Analysis of CFGs
----------------

Echo implements various standard analysis algorithms that can be used to effectively analyse a constructed CFG:

- [Traversal of Control Flow Graphs](Traversals.md)
- [Dominator Analysis](Dominators.md)

Exporting CFGs
--------------

Echo also has the capability to export CFGs:

- [Exporting CFGs to Dot files](Dot.md)