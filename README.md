Echo
====

Echo is an experimental generic, static analysis, symbolic execution and emulation framework, that aims to help out with binary code analysis for a variety of platforms.

Echo is released under the LGPLv3 license.

Build Status
------------

| Branch | Status (Linux)                                                                  |
|--------|---------------------------------------------------------------------------------|
| master | ![Linux](https://github.com/Washi1337/Echo/workflows/Linux/badge.svg)           |


Compiling
---------

Echo can be built using `dotnet build`, or any IDE that is capable of building .NET Standard 2.0 projects (such as Visual Studio or JetBrains Rider).
Well there is some 
Not all projects need to be built for a working binary to be produced. Only the core libraries found in `src/Core` are required to be built. Any other project, such as the platform-specific back-ends in the `src/Platforms` directory and the test projects in `test/`, is optional and can be unloaded safely.


Documentation
-------------
Check out the [wiki](???) for guides and information on how to use the library!

Contributing
------------
See [CONTRIBUTING.md](CONTRIBUTING.md).

Found a bug or have questions?
------------------------------
Please use the [issue tracker](https://github.com/Washi1337/Echo/issues). Try to be as descriptive as possible.

