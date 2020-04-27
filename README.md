Echo
====

Echo is a generic, static analysis, symbolic execution and emulation framework, that aims to help out with binary code analysis for a variety of platforms.

Echo is released under the LGPLv3 license.

Build Status
------------

| Branch | Status (Linux)                                                                  |
|--------|---------------------------------------------------------------------------------|
| master | ![Linux](https://github.com/Washi1337/Echo/workflows/Linux/badge.svg)           |


Compiling
---------

Echo can be built using `dotnet build`, or any IDE that is capable of building .NET Standard 2.0 projects (such as Visual Studio or JetBrains Rider).

Not all projects need to be built for a working binary to be produced. Only the core libraries found in `src/Core` are required to be built. Any other project, such as the platform-specific back-ends in the `src/Platforms` directory and the test projects in `test/`, is optional and can be unloaded safely.


Documentation
-------------
Check out the documentation folder `doc` for guides, structure and roadmap of the Echo project.
