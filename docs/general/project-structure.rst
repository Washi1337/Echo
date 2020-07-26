General Project Structure
=========================

The Echo project mainly consists of two parts. The core libraries that is shared amongst all platforms, and the various back-end libraries that implement the interfaces to describe various platforms.

Core
----

The core libraries expose the base interfaces that all platforms should implement, as well as the analysis algorithms that work on these models.

- **Echo.Core:** The core package, containing base models and interfaces for graph-like structures and program code.

- **Echo.ControlFlow:** The package providing models and algorithms for anything related to control flow analysis. This includes extracting control flow graphs from instruction streams, as well as serialization to blocks.

- **Echo.DataFlow:** The package providing models and algorithms for anything related to data flow analyis. This includes extracting data flow graphs, symbolic values, program slicing and finding dependency instructions.

- **Echo.Concrete:** The package providing base models for emulator packages. This includes the implementation of various concrete emulated values, as well as interfaces and base models for code emulators and interpreters.


Platforms
---------

Different platforms have different features and instruction sets. Therefore, for Echo to do analysis on code targeting such a platform, it needs to know certain properties about what the code looks like, and how a program evolves over time.

A platform typically implements at least the following interfaces from the core libraries:

- **Code model interfaces**: 
    - ``IInstructionSetArchitecture<TInstruction>``: An interface representing an instruction set architecture (ISA) and is used to extract various kinds of information from a single instruction modelled by ``TInstruction``.
    - ``IVariable``: An interface describing a single variable.

- **Emulation interfaces**:
    - ``IProgramState``: An interface describing what the current state of a program at a given point in time might look like. This includes (global) variables, the stack and memory state.