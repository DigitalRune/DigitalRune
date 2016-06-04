# DigitalRune Engine

Copyright (C) DigitalRune GmbH. 
Authors: Helmut Garstenauer, Martin Garstenauer

The **DigitalRune Engine** is a collection of middleware libraries and tools for developing 3D games,
VR simulations and CAx applications. The software is written in C# for the Microsoft .NET Framework
and Mono. It supports the Microsoft XNA Game Studio and MonoGame.

- [Documentation](#documentation)
- [Notes](#notes)
- [Build instructions](#build-instructions)
- [Media](#media)
- [License](#license)


## Documentation

The documentation is available online: [Link](http://digitalrune.github.io/DigitalRune-Documentation/)

Please note that the documentation has not been updated for the open source version.


## Notes

The software was designed to serve as a personal reference implementation. It should be easy to use,
yet rich with features. It was initially written for the Microsoft XNA Framework 4. Several
limitations of the XNA Framework are still present in the software. By focusing on .NET 4.6+, C# 6+,
DirectX 11+ many things could be solved more elegantly.

Several newer, planned features and optimizations are not yet included in the open source project,
for example:
DirectX 11/12 features, physically-based rendering (PBR), temporal reprojection techniques, 
improved game object system (game object behaviors), scripting and NuGet support.

The engine was designed for small to medium games ("indie games").
But there is room for optimizations: You can trim features that are not needed and
optimize where necessary. 

Please note that the original contributors are no longer involved with further development of the
open source project. Currently, the open source project does not have a maintainer. At the moment
we do not accept pull requests. But feel free to fork the project!


### Experimental Features

The solution includes several WPF libraries, an MVVM framework for WPF and an early version of an
IDE (integrated development environment) for games. 
See [DigitalRune Editor](https://github.com/DigitalRune/DigitalRune/wiki/DigitalRune-Editor).


### MonoGame Fork

The DigitalRune Engine uses a forked version of MonoGame: [MonoGame fork for DigitalRune](https://github.com/DigitalRune/MonoGame)

The MonoGame fork contains a few documented changes compared to the original MonoGame. Ideally, 
those changes are merged with the original repository or removed to use the original
MonoGame version directly and get rid of the fork.


## Build instructions

Here are instructions for building the DigitalRune Engine. Please note that there is no automated
build system. (The original build infrastructure is not available for the open source project. Due
to a limited amount of time, it was not possible to set up a new build system.)

Before you start, check the [Prerequisites](http://digitalrune.github.io/DigitalRune-Documentation/html/46419cff-2a6e-4d81-84e4-051800b9727b.htm#Prerequisites).


### How to build the DigitalRune Engine for MonoGame

MonoGame content projects are not included in the Visual Studio solutions. A few manual steps are
required:

1. Update all git submodules recursively (to load the MonoGame submodules).
1. Run `Source/MonoGame/Protobuild.exe` to generate MonoGame project files and solutions.
1. Build the Visual Studio solution *DigitalRune-MonoGame-\<Platform\>.sln* (Configuration: Release, Platforms: Mixed Platforms).
A few projects will fail because the MonoGame content projects haven't been built yet.
1. Build the DigitalRune content by running `Build-Content-Release.cmd`.
1. Build the sample content by running `Samples/Build-Content-MonoGame-<Platform>.cmd`.
1. Build the Visual Studio solution *DigitalRune-MonoGame-\<Platform\>.sln* again.
Now, the projects should build successfully.
1. Run the sample project *Samples/Samples-MonoGame-\<Platform\>.csproj*.


### How to build the DigitalRune Engine for XNA

1. Build the Visual Studio solution *DigitalRune-XNA-Windows*.
1. Run the sample project *Samples/Samples-XNA-Windows.csproj*.

To use the Microsoft XNA Game Studio with Visual Studio 2012 (or newer) follow these instructions:
[Link](http://digitalrune.github.io/DigitalRune-Documentation/html/06ec096e-b312-4052-b0ac-056d89efb5e1.htm)



### How to build the documentation

The DigitalRune assemblies for the XNA Framework are used as the documentation source.
The documentation can be built using the [Sandcastle Help File Builder](https://github.com/EWSoftware/SHFB).

1. Build the Visual Studio solution *DigitalRune-XNA-Windows.sln* (Configuration: Release, Platforms: Mixed Platforms).
1. Build the Sandcastle project *Documentation/Documentation.shfbproj*.

The output can be found in *\_help/*.



## Media

[![Terrain Rendering Example](https://img.youtube.com/vi/hHO6UjbJlP8/0.jpg)](https://www.youtube.com/watch?v=hHO6UjbJlP8)

[![Water Rendering Example](https://img.youtube.com/vi/lUdBm81y1Ik/0.jpg)](https://www.youtube.com/watch?v=lUdBm81y1Ik)

[![Water Rendering Example](https://img.youtube.com/vi/7VgatinyuzE/0.jpg)](https://www.youtube.com/watch?v=7VgatinyuzE)

Have a look at our [YouTube channel](https://www.youtube.com/user/DigitalRuneSoftware/videos) for
more videos.


## License

The DigitalRune Engine is licensed under the terms and conditions of the 3-clause BSD License.
Portions of the code are based on third-party projects which are licensed under their respective
licenses. See [LICENSE.TXT](LICENSE.TXT) for more details.
