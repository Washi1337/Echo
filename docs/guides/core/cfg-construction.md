# Graph Construction

There are two main strategies for constructing control and data flow graphs from a raw code stream: Static and Symbolic Graph Construction.


## Static Graph Construction

The most efficient way to construct a CFG is using a static control flow graph builder.
Each architecture that supports static control flow graph building implements the `IStaticSuccessorResolver` interface, which is able to determine the successors of a single instruction.
A graph can then be constructed using the `StaticFlowGraphBuilder` class:

```csharp
using Echo.ControlFlow.Construction;

IArchitecture<TInstruction> architecture = ...;
IStaticSuccessorResolver<TInstruction> resolver = ...;

IList<TInstruction> instructions = ...;
var builder = new StaticFlowGraphBuilder<TInstruction>(
    architecture, 
    instructions, 
    resolver
);

var cfg = builder.ConstructFlowGraph();
```

The control flow graph builder also supports instruction providers that can obtain or decode instructions by an offset instead.
This can be useful when dealing with architectures for which the start and end of a function is not well-defined and a recursive descent disassembly is required (such as x86):

```csharp
class X86InstructionDecoder : IStaticInstructionProvider<X86Instruction>
{
    X86Instruction GetInstructionAtOffset(long offset) 
    {
        return /* ... Decode instruction at provided offset ... */
    }
}

/* ... */ 

var decoder = new X86InstructionDecoder();
var builder = new StaticFlowGraphBuilder<X86Instruction>(
    architecture, 
    decoder, 
    resolver
);

var cfg = builder.ConstructFlowGraph(entryPoint: 0x1234);
```

> [!NOTE]
> Often, a backend platform has this boilerplate already implemented by extension methods.
> For instance, `Echo.Platforms.AsmResolver` defines an extension method on `CilMethodBody` called `ConstructStaticFlowGraph`.
> ```csharp
> CilMethodBody methodBody = ...;
> var cfg = methodBody.ConstructStaticFlowGraph();
> ```
> Refer to the platform-specific documentation to see how these graphs can be constructed easily.


## Symbolic Graph Construction

A big limitation of static CFG builders is that it cannot work on chunks of code that contain indirect jumps or calls.
Symbolic CFG builders attempt to solve this, by keeping track of data flow as the disassembly is going.
In contrast to a static successor, the symbolic graph builder requires an instance of `IStateTransitioner` instead. 
This interface takes a symbolic input state, and transforms it into a set of all the possible symbolic output states that the instruction may produce.


```csharp
using Echo.DataFlow.Construction;

IArchitecture<TInstruction> architecture = ...;
StateTransitioner<TInstruction> transitioner = ...;

IList<TInstruction> instructions = ...;
var builder = new SymbolicFlowGraphBuilder<TInstruction>(
    architecture, 
    instructions, 
    transitioner
);

var cfg = builder.ConstructFlowGraph();
```

Most state transitioners produce a data flow graph as a by-product.

```csharp
// First create the CFG.
var cfg = builder.ConstructFlowGraph();

// After building the CFG, a DFG is populated in the transitioner.
var dfg = transitioner.DataFlowGraph;
```

> [!WARNING]
> While symbolic graph construction usually is more accurate, it is significantly slower than static graph construction and can take a lot of memory.


> [!NOTE]
> Often, a backend platform has this boilerplate already implemented by extension methods.
> For instance, `Echo.Platforms.AsmResolver` defines an extension method on `CilMethodBody` called `ConstructStaticFlowGraph`.
> ```csharp
> CilMethodBody methodBody = ...;
> var cfg = methodBody.ConstructSymbolicFlowGraph(out var dfg);
> ```
> Refer to the platform-specific documentation to see how these graphs can be constructed easily.
