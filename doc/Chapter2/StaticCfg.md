Static Control Flow Graph Builders
==================================

The easiest, and probably most efficient way to construct a control flow graph using Echo is using a static control flow graph builder. This is generally good enough for simple memory-safe instruction sets such as CIL and the JVM.

Example
-------
To construct a CFG using the static control flow graph builder, we need classes from the following namespace:
```csharp
using Echo.ControlFlow.Construction;
```

The static graph builder of Echo uses an `IInstructionProvider<TInstruction>` to access the instructions it stores in the graph.

Suppose you want to construct a control flow graph of a collection of instructions of type `DummyInstruction`, stored in variable `instructions` (of type `IEnumerable<DummyInstruction>` or one of its derivatives). To wrap it in an instruction provider, we can use the `InstructionList` class.

```csharp
var list = new InstructionList<DummyInstruction>(instructions);
```

Next, we need an instance of `IStaticSuccessorResolver<TInstruction>`, which is able to tell the graph builder what the successors are of a single instruction. This is platform dependent, and every platform has their unique successor resolver.

```csharp
var resolver = new DummyStaticSuccessorResolver();
```

Now we can build our control flow graph from our list. Given the entrypoint address stored in a variable `entrypointAddress` of type `long`, we can construct the control flow graph using:
```csharp
var builder = new StaticGraphBuilder<DummyInstruction>(list, resolver);
Graph<DummyInstruction> graph = builder.ConstructFlowGraph(entrypointAddress);

```

How it works
------------

A static control flow graph builder performs a recursive traversal over all instructions, starting at a provided entrypoint, and adds for every branching opcode an edge in the control flow graph. By repeatedly using the provided `IInstructionProvider` and the `IStaticSuccessorResolver` instances, it collects every instruction and determines the outgoing edges of each basic block.

The reason why a separate interface `IInstructionProvider<TInstruction>` is used over a normal `IEnumerable<TInstruction>`, is because a normal list might not always be the most efficient data structure to obtain instructions at very specific offsets. Furthermore, this also allows for disassemblers to implement this interface, and decode instructions on-the-fly while simultanuously building the control flow graph.

This means it is a very efficient algorithm that scales linearly in the number of instructions to be processed, and it is usually enough for most simple instruction sets such as CIL from .NET or bytecode from the JVM, and could work for a lot of cases of native platforms such as x86.

Limitations
-----------
A big limitation of this approach, however, is that it cannot work on chunks of code that contain indirect jumps or calls. These might occur in for example chunks of x86 code such as the following:

```x86
mov eax, address
jmp eax
```

Since the static graph builder does not do any data flow analysis or emulation of the code, this basic block will produce a dead end in the final graph. 

If this is a problem, dynamic graph builders (based on symbolic execution or emulation) might be more suited for the job, but might be significantly slower or expose the user to a risk of running arbitrary code on their own machine.