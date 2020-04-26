Symbolic Control Flow Graph Builders
====================================

For some architectures, static recursive traversal of a procedure is not enough. Jump instructions might use the stack or registers to determine the branch target. For this, further analysis is needed and symbolic control flow graph builders could help.

Example
-------

To construct a CFG using the symbolic control flow graph builder, we need classes from the following namespace:

```csharp
using Echo.ControlFlow.Construction.Symbolic
```

Similar to the static graph builder, the symbolic graph builder of Echo defines one constructor that accepts an instruction set architecture, and a transition resolver. The architecture tells Echo how to interpret a model of an instruction in the instruction set. The transition resolver is able to tell the graph builder how a certain instruction behaves and what its possible effects are, given a program state. Both parameters are platform dependent, and every platform has their unique architecture representative and transition resolver.

In the following snippets, we assume our instruction model is called `DummyInstruction`, our architecture is represented by `DummyArchitecture`, and our transition resolver is of the type `DummyTransitionResolver`:

```csharp
public readonly struct DummyInstruction { ... } 
public class DummyArchitecture : IInstructionSetArchitecture<DummyInstruction> { ... }
public class DummyTransitionResolver : IStateTransitionResolver<DummyInstruction> { ... }

// ...

var architecture = mew DummyArchitecture();
var resolver = new DummyTransitionResolver(architecture);
```

### Preparing the Instructions

Next, we need our instructions. We can use any instance of `IEnumerable<TInstruction>` to instantiate a `SymbolicFlowGraphBuilder`:
```csharp
var instructions = new List<DummyInstruction> { ... }
var builder = new SymbolicFlowGraphBuilder<DummyInstruction>(architecture, instructions, resolver);
```

or use an instance of a class implementing the `IStaticInstructionProvider<TInstruction>` interface:

```csharp
var instructions = new ListInstructionProvider<DummyInstruction>(architecture, new List<DummyInstruction> { ... });
var builder = new SymbolicFlowGraphBuilder<DummyInstruction>(instructions, resolver);
```

If decoding instructions requires more than just the current value of the program counter register, it is also possible to specify an `ISymbolicInstructionProvider<TInstruction>` instead. This takes an entire program state instead of just the program counter.

```csharp
ISymbolicInstructionProvider<TInstruction> instructions = ...
var builder = new SymbolicFlowGraphBuilder<DummyInstruction>(instructions, resolver);
```

### Building the graph

Building our control flow graph is then very similar to the static control flow graph builder. Given the entrypoint address stored in a variable `entrypointAddress` of type `long`, we can construct the control flow graph using:

```csharp
ControlFlowGraph<DummyInstruction> cfg = builder.ConstructFlowGraph(entrypointAddress);
```

A nice by-product of most symbolic transition resolvers is that it automatically also creates a data flow graph during the traversal of instructions.

```csharp
DataFlowGraph<DummyInstruction> dfg = resolver.DataFlowGraph;
```

How it works
-------------

The symbolic graph builder traverses instructions in a similar way as a normal static graph builder would do. The difference is that while traversing, it maintains a symbolic state of the program, where it keeps track of the current state of the stack and variables as if the instructions were executed. Keep in mind though that the program state is fully symbolic, and does not actually execute the instructions.

This approach allows for transition resolvers to look at the current program state, and infer from this any indirect branch target or other unconventional behaviour.

For example, an x86 back-end could resolve the branch target of the following jump instruction, by looking at the data dependencies and recognising that the value of `eax` will contain the value `0x12345678` at runtime.

```asm
mov eax, 0x12345678
jmp eax
```

The downside is that because the symbolic graph builder needs to keep track of all the changes in a method body, it can be significantly slower and needs a lot more memory. Furtheremore, sometimes the builder might have to revisit a few instructions if more information has been obtained. Therefore, if no data flow graph is needed for the use case, it is recommended to use a static flow graph builder instead.