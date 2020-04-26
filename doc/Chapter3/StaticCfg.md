Static Control Flow Graph Builders
==================================

The easiest, and probably most efficient way to construct a control flow graph using Echo is using a static control flow graph builder. This is generally good enough for simple memory-safe instruction sets such as CIL and the JVM.

Example
-------
To construct a CFG using the static control flow graph builder, we need classes from the following namespace:
```csharp
using Echo.ControlFlow.Construction.Static;
```

The static graph builder of Echo defines one constructor that accepts an instruction set architecture, and a successor resolver. The architecture tells Echo how to interpret a model of an instruction in the instruction set. The successor resolver is able to tell the graph builder what the successors are of a single instruction. Both parameters are platform dependent, and every platform has their unique architecture representative and successor resolver.

In the following snippets, we assume our instruction model is called `DummyInstruction`, our architecture is represented by `DummyArchitecture`, and our successor resolver is of the type `DummyStaticSuccessorResolver`:

```csharp
public readonly struct DummyInstruction { ... } 
public class DummyArchitecture : IInstructionSetArchitecture<DummyInstruction> { ... }
public class DummyStaticSuccessorResolver : IStaticSuccessorResolver<DummyInstruction> { ... }

// ...

var architecture = mew DummyArchitecture();
var resolver = new DummyStaticSuccessorResolver(architecture);
```

### Preparing the Instructions

Next we need a set of instructions to graph. We can use any instance of `IEnumerable<TInstruction>` to instantiate a `StaticFlowGraphBuilder`:
```csharp
var instructions = new List<DummyInstruction> { ... }
var builder = new StaticFlowGraphBuilder<DummyInstruction>(architecture, instructions, resolver);
```

or use an instance of a class implementing the `IStaticInstructionProvider<TInstruction>` interface instead:

```csharp
var instructions = new ListInstructionProvider<DummyInstruction>(architecture, new List<DummyInstruction> { ... });
var builder = new StaticFlowGraphBuilder<DummyInstruction>(instructions, resolver);
```

### Building the graph

Now we can build our control flow graph from our list. Given the entrypoint address stored in a variable `entrypointAddress` of type `long`, we can construct the control flow graph using:
```csharp
ControlFlowGraph<DummyInstruction> graph = builder.ConstructFlowGraph(entrypointAddress);
```

How it works
------------

A static control flow graph builder performs a recursive traversal over all instructions, starting at a provided entrypoint, and adds for every branching opcode an edge in the control flow graph. By repeatedly using the provided `IStaticInstructionProvider` and the `IStaticSuccessorResolver` instances, it collects every instruction and determines the outgoing edges of each basic block.

The reason why a separate interface `IStaticInstructionProvider<TInstruction>` is used over a normal `IEnumerable<TInstruction>`, is because a normal list might not always be the most efficient data structure to obtain instructions at very specific offsets. Furthermore, this also allows for disassemblers to implement this interface, and decode instructions on-the-fly while simultanuously building the control flow graph.

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