# Echo

Echo is an experimental generic, static analysis, symbolic execution and emulation framework, that aims to help out with binary code analysis for a variety of platforms or backends.

Echo is released under the LGPLv3 license.


## Main Features

- [x] Generic Graph Models
  - [x] Traversal and structural detection algorithms
  - [x] Serialization to Dot/GraphViz
- [x] Generic Control flow Analysis
    - [x] Create static and symbolic flow graphs
    - [x] Dominator analysis
    - [x] Serialize into scoped flow blocks or a list of instructions
- [x] Generic Data flow Analysis
    - [x] Create data flow graphs
    - [x] Inspect stack and variable dependencies of instructions
- [x] Generic AST Construction
    - [x] Lift control flow graphs to Abstract Syntax Trees (ASTs)
    - [x] Automatic variable cross-referencing
- [x] Generic Emulation Engine Framework
  - [x] Virtual memory model using low level bit vectors
  - [x] Support for HLE and LLE arithmetic on fully known, partially known and fully unknown bit vectors of any size


## Supported Backends:

| Architecture | Back-end                                                | Control Flow | Data Flow | AST     | Purity Classification | Emulation |
|--------------|---------------------------------------------------------|--------------|-----------|---------|-----------------------|-----------|
| CIL          | [AsmResolver](https://github.com/Washi1337/AsmResolver) | ✓            | ✓         | ✓       | ✓                     | ✓ (WIP)   |
| CIL          | [dnlib](https://github.com/0xd4d/dnlib)                 | ✓            | ✓         | ✓       | ✓                     |           |
| x86 (32-bit) | [Iced](https://github.com/icedland/iced)                | ✓            | ✓         | ✓ (WIP) |                       |           |
| x86 (64-bit) | [Iced](https://github.com/icedland/iced)                | ✓            | ✓         | ✓ (WIP) |                       |           |


## Binaries

- [Nightly NuGet Feed](https://nuget.washi.dev/)

| Branch | Status (Linux)                                                                  |
|--------|---------------------------------------------------------------------------------|
| master | ![Linux](https://github.com/Washi1337/Echo/workflows/Linux/badge.svg)           |


## Compiling

Simply run 

```
dotnet build
```

Alternatively, use any IDE that is capable of building .NET Standard 2.0 projects (such as Visual Studio or JetBrains Rider).

Not all projects need to be built for a working binary to be produced. Only the core libraries found in `src/Core` are required to be built. Any other project, such as the platform-specific back-ends in the `src/Platforms` directory and the test projects in `test/`, is optional and can be unloaded safely.


## Documentation

Check out the [wiki](https://echo-emu.readthedocs.io/) for guides and information on how to use the library!


## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).


## Found a bug or have questions?

Please use the [issue tracker](https://github.com/Washi1337/Echo/issues). Try to be as descriptive as possible.