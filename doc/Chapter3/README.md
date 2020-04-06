Control Flow Analysis
=====================

What are Control Flow Graphs?
-----------------------------

A control flow graph (CFG) is a graph where each node corresponds to one basic block of code, and each edge represents a possible control flow transfer. This way, a control flow graph tries to encode all possible execution paths in a chunk of code, method or entire program.

The figure below depicts a control flow graph of a very simple if-statement:

![If statement](img/if.png)

How are they modelled in Echo?
------------------------------

There are three main classes that are used to model CFGs. Given a `TInstruction` type, representing the type of instructions to store in the CFG, Echo defines the following classes:
- `ControlFlowNode<TInstruction>`: A single node containing a basic block of instructions.
- `ControlFlowEdge<TInstruction>`: A control flow transfer between two nodes.
- `ControlFlowGraph<TInstruction>`: A collection of control flow nodes and edges.

These classes implement the `INode`, `IEdge` and `IGraph` interfaces, and work therefore with all kinds of generic graph algorithms and export features.

CFGs can also be subdivided into multiple regions. These are represented using the `IControlFlowRegion<TInstruction>` interface, and are accessible through the `Regions` property of the `ControlFlowGraph<Tinstruction>` class. Echo provides the following base implementations:
- `BasicControlFlowRegion<TInstruction>`: A basic collection of nodes.
- `ExceptionHandlerRegion<TInstruction>`: A region representing an exception handler, consisting of a protected region and a collection of handler regions. More about exception handlers [here](ExceptionHandlers.md).


Constructing CFGs
-----------------

Echo supports constructing control flow graphs from chunks of code for a variety of platforms, provided that these platforms implement just a couple of interfaces.

- [Static CFG construction](StaticCfg.md)
- [Symbolic CFG construction](SymbolicCfg.md)
- [Exception handler detection](ExceptionHandlers.md)

Analysis of CFGs
----------------

Echo implements various standard analysis algorithms that can be used to effectively analyse a constructed CFG:

- [Traversal of Control Flow Graphs](Traversals.md)
- [Dominator Analysis](Dominators.md)
