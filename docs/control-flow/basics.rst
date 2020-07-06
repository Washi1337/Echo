The Basics
==========

What are Control Flow Graphs?
-----------------------------

A control flow graph (CFG) is a graph where each node corresponds to one basic block of code, and each edge represents a possible control flow transfer. This way, a control flow graph tries to encode all possible execution paths in a chunk of code, method or entire program.

The figure below depicts a control flow graph of a very simple if-statement:

.. image:: img/if.png
    :width: 200px
    :align: center
    :height: 100px
    :alt: A control flow graph of an if statement.

Control Flow Graph Models
-------------------------

There are three main classes that are used to model CFGs. Given a ``TInstruction`` type, representing the type of instructions to store in the CFG, Echo defines the following classes:

- ``ControlFlowNode<TInstruction>``: A single node containing a basic block of instructions.
- ``ControlFlowEdge<TInstruction>``: A control flow transfer between two nodes.
- ``ControlFlowGraph<TInstruction>``: A collection of control flow nodes and edges.

These classes implement the ``INode``, ``IEdge`` and ``IGraph`` interfaces, and work therefore with all kinds of generic graph algorithms and export features.

CFGs can also be subdivided into multiple regions. These are represented using the ``IControlFlowRegion<TInstruction>`` interface, and are accessible through the ``Regions`` property of the ``ControlFlowGraph<Tinstruction>`` class. Echo provides the following base implementations:

- ``BasicControlFlowRegion<TInstruction>``: A basic collection of nodes.
- ``ExceptionHandlerRegion<TInstruction>``: A region representing an exception handler, consisting of a protected region and a collection of handler regions.


Inspecting basic blocks
-----------------------

A single ``ControlFlowNode<TInstruction>`` contains a single basic block of code. A basic block is a sequence of contiguous instructions that contains no jumps or labels. To access the basic block stored in the node, use the ``Contents`` property, which is of type ``BasicBlock<TInstruction>``:

.. code-block:: csharp

    long offset = ...
    var node = cfg.GetNodeByOffset(offset);
    var block = node.Contents;

    // Iterate over all instructions within the block.
    foreach (var instruction in block.Instructions) 
        Console.WriteLine(instruction);

    // Get immedaitely the header/footer of the basic block.
    var firstInstruction = block.Header;
    var lastInstruction = block.Footer;
    

Inspecting incoming and outgoing edges
--------------------------------------

Control flow nodes might transfer control to another block within the graph. To model this, instances of ``ControlFlowNode<TInstruction>`` define various properties that allow access to edges that are incident to this node.

There are three types of edges in a control flow graph:

- ``FallthroughEdge:`` The default edge that is taken when no other edge is taken.
- ``ConditionalEdges:`` Edges that are only taken when a particular condition is met.
- ``AbnormalEdges:`` Edges that are taken in the case of a rare event (such as exceptions).

These three properties are fully mutable, and can therefore be used to add or remove any edge within the graph to change the flow of a program.

However, the recommended way to add new edges is using the ``ConnectWith`` method:

.. code-block:: csharp

    var node1 = cfg.GetNodeByOffset(offset1);
    var node2 = cfg.GetNodeByOffset(offset2);
    var node3 = cfg.GetNodeByOffset(offset3);

    // Adds a fallthrough edge from node1 to node2.
    node1.ConnectWith(node2);
    // Adds a conditional edge from node1 to node2.
    node1.ConnectWith(node3, ControlFlowEdgeType.Conditional);

Updating any of the three properties will automatically update the return value of the``GetIncomingEdges()`` method of the target node.

.. code-block:: csharp

    var node1 = cfg.GetNodeByOffset(offset1);
    var node2 = cfg.GetNodeByOffset(offset2);

    // Adds a fallthrough edge from node1 to node2.
    node1.ConnectWith(node2);

    var incomingEdge = node2.GetIncomingEdges().First();

