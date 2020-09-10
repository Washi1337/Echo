Blocks
======

An important feature of Echo's control flow analysis package is the representation of code blocks. Blocks describe the structural hierarchy and context of nodes in a control flow graph, and can be used to get more insight about which nodes belong to each other, and in which order blocks appear. They also form an integral part in the serialization from control flow graphs back to a raw sequence of instructions.


Types of blocks
---------------

Echo defines three types of blocks that might appear in a block tree, which all implement the ``IBlock<TInstruction>`` interface:

- ``BasicBlock``: Represents a sequence of instructions that when executed, is executed in its entirety, and can only have incoming branches at the very beginning of the block, and can only introduce outgoing branches by the last instruction in the block.
- ``ScopeBlock``: Represents a sequence of blocks that are put into a lexical scope. A lexical scope block can contain any other types of blocks, including nested scope blocks.
- ``ExceptionHandlerBlock``: Represents a scope block that is protected by one or more exception handlers.

The entirety of a function's body is usually represented using a single ``ScopeBlock``. To get all basic blocks (leafs in the blocks tree), use the ``GetAllBlocks`` method. This can for example be used to get all instructions in a block tree.

.. code-block:: csharp

    IBlock<TInstruction> someBlock = ...;

    foreach (BasicBlock<TInstruction> basicBlock in someBlock.GetAllBlocks())
    {
        Console.WriteLine($"Block_{basicBlock.Offset:X}:);

        foreach (var instruction in basicBlock.Instructions)
            Console.WriteLine(instruction);
            
        Console.WriteLine();
    }

Constructing Blocks from Control Flow Graphs
--------------------------------------------

Constructing structural blocks can be done by using the ``BlockBuilder`` class, defined in the ``Echo.ControlFlow.Serialization.Blocks`` interface. Below an example on how to construct a ``ControlFlowGraph<TInstruction>`` into an instance of ``ScopeBlock<TInstruction>``:

.. code-block:: csharp

    ControlFlowGraph<TInstruction> cfg = ...;

    BlockBuilder<TInstruction> builder = new BlockBuilder<TInstruction>();
    ScopeBlock<TInstruction> rootScope = builder.ConstructBlocks(cfg);


.. warning:: 
    
    While the block builder sorts blocks in a control flow graph in a topological sorting, it does not introduce any branch instructions that might be needed to correctly connect the blocks.


Block visitors and listeners
----------------------------

The blocks API implements the visitor pattern. That is, once a ``ScopeBlock<TInstruction>`` is constructed, it can be traversed using the ``IBlockVisitor<TInstruction>`` interface, which defines methods for each type of block that might appear in a blocks tree. Every type of block defines the ``AcceptVisitor`` method, that calls the corresponding visitor method in the ``IBlockVisitor<TInstruction>`` interface.

Since a pre-order traversal is one of the most common traversals of a block tree, this is implemented using the ``BlockWalker<TInstruction>`` class. To get ahold of all the blocks that are entered and exit, the block walker takes any instance of ``IBlockListener<TInstruction>`` that defines ``Enter`` and ``Exit`` methods for every type of block. An example implementation is the ``BlockFormatter`` class, which converts every encoutered block to a human-readable string.

.. code-block:: csharp

    IBlock<TInstruction> someBlock = ...;

    var formatter = new BlockFormatter<TInstruction>();
    var walker = new BlockWalker(formatter);
    walker.Walk(someBlock);

    string output = formatter.GetOutput();
    Console.WriteLine(output);