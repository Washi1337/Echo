Echo
====

Echo is an experimental generic, static analysis, symbolic execution and emulation framework, that aims to help out with binary code analysis for a variety of platforms.

Echo is released under the LGPLv3 license.

Main Features
-------------
- Control flow analysis
    - Create static and symbolic flow graphs
    - Dominator analysis
    - Serialize into scoped flow blocks or a list of instructions
- Data flow analysis
    - Create data flow graphs
    - Inspect stack and variable dependencies of instructions.
- Unified generic API.
    - Serialize any kind of graph to the dot file format.
    - Adding a new platform for flow analysis requires minimal effort
- Supported platforms:
    - AsmResolver (CIL)
    - dnlib (CIL)
    - Iced (x86 32-bit and 64-bit)

Compiling
---------

Echo can be built using `dotnet build`, or any IDE that is capable of building .NET Standard 2.0 projects (such as Visual Studio or JetBrains Rider).

Not all projects need to be built for a working binary to be produced. Only the core libraries found in `src/Core` are required to be built. Any other project, such as the platform-specific back-ends in the `src/Platforms` directory and the test projects in `test/`, is optional and can be unloaded safely.


Build Status
------------

| Branch | Status (Linux)                                                                  |
|--------|---------------------------------------------------------------------------------|
| master | ![Linux](https://github.com/Washi1337/Echo/workflows/Linux/badge.svg)           |

Documentation
-------------
Check out the [wiki](https://echo-emu.readthedocs.io/) for guides and information on how to use the library!

Contributing
------------
See [CONTRIBUTING.md](CONTRIBUTING.md).

Found a bug or have questions?
------------------------------
Please use the [issue tracker](https://github.com/Washi1337/Echo/issues). Try to be as descriptive as possible.

