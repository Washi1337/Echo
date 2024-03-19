# Block Trees

An important feature of Echo's control flow analysis package is the representation of code blocks. 
Blocks describe the structural hierarchy and context of nodes in a control flow graph, and can be used to get more insight about which nodes belong to each other, and in which order blocks appear. 
They also form an integral part in the serialization from control flow graphs back to a raw sequence of instructions.


## Block Types

Echo defines types of blocks that might appear in a block tree, which all implement the ``IBlock<TInstruction>`` interface:

| Type                    | Description                                                         |
|-------------------------|---------------------------------------------------------------------|
| `BasicBlock`            | A block containing a sequence of instructions executed as one unit. |
| `ScopeBlock`            | A block defining a scope.                                           |
| `ExceptionHandlerBlock` | A scope protected by one or more exception handlers.                |
| `HandlerBlock`          | A single exception handler block.                                   |

A single `BasicBlock` contains sequence of instructions.

```csharp
// Define a new basic block.
var block = new BasicBlock<TInstruction>();

// Add instructions.
block.Instructions.Add(instruction1);
block.Instructions.Add(instruction2);
block.Instructions.Add(instruction3);
```

Blocks can be put into a scope block:

```csharp
var scope = new ScopeRegion<TInstruction>();
scope.Blocks.Add(block);
```

Blocks can be protected by one or more exception handlers:

```csharp
var eh = new ExceptionHandlerBlock<TInstruction>();
eh.ProtectedBlock.Blocks.Add(block1);

var handler = new HandlerBlock<TInstruction>();
handler.Contents.Blocks.Add(block2);
eh.Handlers.Add(handler);
```


## Constructing Blocks

Constructing structural blocks can be done by creating a [control flow graph](cfg-construction.md) first, and then using the `ConstructBlocks` extension method:

```csharp
using Echo.ControlFlow.Serialization.Blocks;

ControlFlowGraph<TInstruction> cfg = ...;
ScopeBlock<TInstruction> rootScope = cfg.ConstructBlocks();
```

> [!WARNING]
> While the block builder sorts blocks in a control flow graph in a topological sorting, it does not introduce any branch instructions that might be needed to correctly connect the blocks.


## Traversing Block Trees

The blocks API implements the visitor pattern. 
Once a ``ScopeBlock<TInstruction>`` is constructed, it can be traversed using the ``IBlockVisitor<TInstruction>`` or `IBlockListener<TInstruction>` interface, which defines methods for each type of block that might appear in a blocks tree. 

```csharp
class MyListener : BlockListenerBase<TInstruction>
{
    public override void EnterScopeBlock(ScopeBlock<TInstruction> scope)
    {
        Console.WriteLine("Entering scope...");
    }

    public override void LeaveScopeBlock(ScopeBlock<TInstruction> scope)
    {
        Console.WriteLine("Leaving scope...");
    }

    public override void VisitBasicBlock(BasicBlock<TInstruction> block) { ... }
}
```

A `BlockWalker` could then be used in conjuction with such a listener to fully traverse the block tree and operate:

```csharp
var walker = new BlockWalker(new MyListener());
walker.Walk(someBlock);
```

## Pretty Printing Blocks

Blocks can be formatted into strings either using the `ToString` method, or using a custom `BlockFormatter` instance.

```csharp
ScopeBlock<TInstruction> root = ...;
Console.WriteLine(root.ToString());
```
