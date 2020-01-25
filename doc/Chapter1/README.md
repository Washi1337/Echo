General Project Structure
=========================

The Echo project mainly consists of two parts. The core libraries that is shared amongst all platforms, and the various back-end libraries that implement the interfaces to describe various platforms.

Core
----
The core libraries expose the base interfaces that all platforms should implement, as well as the analysis algorithms that work on these models.


Platforms
---------
Different platforms have different features and instruction sets. Therefore, for Echo to do analysis on code targeting such a platform, it needs to know certain properties about what the code looks like, and how a program evolves over time.

A platform typically implements at least the following interfaces from the core libraries:
- Code model interfaces: 
    - `IInstructionSetArchitecture<TInstruction>`: An interface representing an instruction set architecture (ISA) and is used to extract various kinds of information from a single instruction modelled by `TInstruction`
    - `IVariable`: An interface describing a single variable.
- Emulation interfaces:
    - `IProgramState`: An interface describing what the current state of a program at a given point in time might look like. This includes (global) variables, the stack and memory state.
- Type system interfaces:
    - `IType`, `IMethod` and `IField`: Some platforms might have a rich type system with self-describing features. Prime examples are the CLR and the JVM. These platforms would implement these interfaces as well.