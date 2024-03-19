# Base Library

The core libraries expose the base interfaces that all platforms should implement, as well as the analysis algorithms that work on these models.
It consists of the following packages:

- **Echo:** 
  The core package, containing base contracts for graph-like structures, program code and memory models.

- **Echo.ControlFlow:** 
  The package responsible for performing control flow analysis.
  This includes extracting control flow graphs from instruction streams, as well as structuring control flow nodes into block trees and traversing them.

- **Echo.DataFlow:** 
  The package responsible for performing data flow analyis. 
  This includes extracting data flow graphs, symbolic values, program slicing and finding dependency instructions.

- **Echo.Ast:** 
  The package responsible for lifting raw instruction streams into abstract syntax trees (AST).