# CIL Graph Construction

The AsmResolver backend supports both two forms of control flow graph construction for CIL method bodies, both accessible by calling one of the extension methods on the `CilMethodBody` class.
In turn, the platform also supports lifting to syntax trees.

## Control Flow Graphs

A static traversal can be done via `ConstructStaticFlowGraph`:

```csharp
using Echo.Platforms.AsmResolver;

MethodDefinition method = ...;
ControlFlowGraph<CilInstruction> cfg = method.CilMethodBody.ConstructStaticFlowGraph();
```

A symbolic traversal that also produces a data flow graph can be performed via `ConstructSymbolicFlowGraph`:

```csharp
using Echo.Platforms.AsmResolver;

MethodDefinition method = ...;
ControlFlowGraph<CilInstruction> cfg = method.CilMethodBody.ConstructSymbolicFlowGraph(out DataFlowGraph<CilInstruction> dfg);
```

Both of these methods also automatically cluster nodes residing in exception handlers into exception handler regions.
Every `HandlerRegion<CilInstruction>` has a `Tag` property containing the original `CilExceptionHandler` instance the handler was based on.

For more information on how to use control and data flow graphs, see [Control Flow Graphs (CFG)](../core/cfg-basics.md) and [Data Flow Grpahs (DFG)](../core/dfg-basics.md).


## Syntax Trees

Syntax trees can be build by first constructing a control flow graph (static traversal is sufficient), and then calling the `Lift` extension method with a `CilPurityClassifier` instance.
A rooted compilation unit can then be built by calling `ToCompilationUnit` on the resulting lifted control flow graph:

```csharp
MethodDefinition method = ...;
CompilationUnit<CilInstruction> ast = method.CilMethodBody
    .ConstructStaticFlowGraph()
    .Lift(new CilPurityClassifier())
    .ToCompilationUnit();
```

For more information on how to use abstract syntax trees, see [Abstract Syntax Trees (AST)](../core/ast-basics.md).
