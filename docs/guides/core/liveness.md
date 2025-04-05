# Liveness Analysis

Liveness analysis is a data-flow analysis on a control flow graph that determines which variables are live at every point in the program.
A variable is live at a point if it holds a value that may be used by some subsequent instruction in the function.

Consider the following example:

```
x := 5
y := 6
foo(x)
z := 7
bar(z)
```

Liveness analysis can tell you that variable `x` is still alive at line 2 (i.e., because it is used in line 3 as an argument), and also tell you that `y` is always dead because it is never used.
It will also tell you that `x` dies after executing line 3, because it is no longer used and thus does not interfere with `z`.

This type of analysis is therefore often used in (de)compiler passes, such as register allocation and dead-code elimination.


## Computing Liveness

Given a `ControlFlowGraph<TInstruction>` instance, liveness analysis can be computed using the `LivenessAnalysis` class:

```csharp
ControlFlowGraph<TInstruction> cfg = ...;

var analysis = LivenessAnalysis.FromFlowGraph(cfg);
```

Liveness can be computed on CFGs containing raw instructions as well as CFGs containing ASTs.


## Using Liveness Information

After analysis, each instruction in the control flow graph that is reachable from the entry point is annotated with a `LivenessData` instance:

```csharp
TInstruction instruction = ...;

var liveness = analysis[instruction];
Console.WriteLine($"In:  {string.Join(", ", liveness.In)}");
Console.WriteLine($"Out: {string.Join(", ", liveness.Out)}");
```

Each liveness data is a tuple of the two sets `In` and `Out`, which contain the currently live variables before and after the instruction execution.
Therefore, a variable dies at the current instruction if it appears in `In` but not in `Out`.


## Constructing Interference Graphs

An interference graph is a graph containing all used variables in the program, and links each pair of variables together if they interfere with each other.
Variables interfere if they can both be live at the same time, and thus cannot be stored at the same location in memory or register.
Interference graphs form a crucial primitive for many register allocation algorithms.

An interference graph can be constructed from a liveness analysis report.

```csharp
LivenessAnalysis liveness = ...;
var graph = InterferenceGraph.FromLiveness(liveness);
```

Variables in the graph can be queried by the `Nodes` property:

```csharp
IVariable variable = ...;
InterferenceGraph graph = ...;

var node = graph.Nodes[variable];
```

Its interference can then be inspected via its `Interference` property:

```csharp
var node = graph.Nodes[variable];

Console.WriteLine($"Variable {variable} interferes with:")
foreach (var n in node.Interference)
    Console.WriteLine(n.Variable);
```