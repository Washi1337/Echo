# Echo

This is the documentation of the Echo project. Echo is a generic, static analysis, symbolic execution and emulation framework, that aims to help out with binary code analysis for a variety of platforms.

The main goals of Echo include but are not limited to the following:

- To provide a model for program code of a variety of platforms.
- To analyse the control and data flow within chunks of code, and to lift raw instruction streams to abstract syntax trees (AST).
- To (symbolically) emulate program code where the entire state of the program is not always fully known.