Introduction
============

What is Project Echo? (short version)
-------------------------------------

Echo is a high-level static and dynamic analysis framework, providing models and algorithms for analysing control and data flow of low-level assembly-like code, as well as symbolic execution and emulation engines for a variety of platforms.


OK but now in normal English (long version)
-------------------------------------------
In software reverse engineering, we often try to figure out what a particular piece of code does. 
We generally use static analysis tools such as disassemblers, that not only decode instructions but also perform some analysis on them, such as the construction of control and data flow graphs.

Over the past years, a lot of different architectures have been developed, each with their own instruction sets. Simultanuously, lots of static analysis libraries have been developed to decode and analyse code targeting these platforms.

The problem is that most of these libraries only target one single platform, and are often incompatible with each other. 
As a result, lots of the standard analysis algorithms (such as the aforementioned control and data flow analysis) has to be adapted or completely recoded to accomodate for all the differences in implementation.
This especially becomes an issue when we look at really obscure instruction sets, such as the ones provided by old processors, or virtual machines used by obfuscators. 
Writing static analysis tools for these kinds of platforms with the same kind of capabilities as the ones for more well-known platforms, would require lots of time and effort, even though the algorithms are more or less the same.

Project Echo tries to solve this by abstracting away the underlying instruction set through a set of interfaces that try to capture all similarities between different platforms. 
A developer could then "simply" implement these interfaces and describe _what_ a specific platform looks like, and not having to worry about how to implement complicated analysis algorithms for their instruction set.

Goals
-----

The goals of Project Echo include (but are not limited to):
- To provide a model for program code of a variety of platforms.
- To provide a framework that models the state of a program, including variables, stack, and memory. 
- To provide the ability to analyse the control and data flow within chunks of code.
- To provide the means to emulate (parts of) the program code, and simplify expressions using symbolic execution and SAT solvers.
- To provide the means to export control flow graphs to various file formats.

