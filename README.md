# Introduction

This repository provides an interpreter for a homemade `procedural object-oriented semi-dynamic typed` programming language, called `BaZic`.
Made entirely in C#, this was initially a project designed to learn how to make a programming language, from the `lexer` to the `interpreter` with a `parser` and `optimizer` in the middle.

# Build Status

[![Build status](https://ci.appveyor.com/api/projects/status/apym7r6knjak8xk2/branch/master?svg=true)](https://ci.appveyor.com/project/veler/bazic/branch/master)

# Downloads

BaZic is available as [NuGet package](https://www.nuget.org/packages/BaZic/). Usage details, documentation and more can be found below.

# Documentation

## BaZic Language

* [Learn about the BaZic language](/Docs/BaZic.md)
* [How to use the BaZic interpreter](/Docs/BaZic_Interpreter.md)

```
VARIABLE initialValue = 100

EXTERN FUNCTION Main(args[])
    RETURN FirstMethod(initialValue) # This must return 0.
END FUNCTION

FUNCTION FirstMethod(num)
    IF num > 1 THEN
        RETURN FirstMethod(num - 1)
    END IF
    RETURN num
END FUNCTION
```

## Technical Documentation

* [Overview](/Docs/Technical_Overview.md)

# Getting Started

## Requirements

1. Windows 7 SP1 or later, or Windows Server 2008 R2 SP1 or later.
3. .NET Framework 4.7.1
4. [KB4033342](http://support.microsoft.com/kb/4033342), [KB4041083](http://support.microsoft.com/kb/4041083) and [KB4049016](http://support.microsoft.com/kb/4049016).

Note : BaZic is `not` compatible with Windows 8, 8.1 and 10 for ARM and is not compatible with Windows 10 "S". It is also not supported by .Net Core (yet?).

## Installation & Run

Simply run `BaZic.Sample.exe`.

![Play](/Docs/Play.png)

# Build and Test

## Setup development environment

### Requirements

1. Windows 10 Pro or Enterprise.
2. Visual Studio 2017 Pro or higher with the following features enabled :
    * .NET Framework 4.7.1 SDK
    * .NET Framework 4.7.1 targeting pack
    * Git for Windows
    * NuGet package manager
    * Text Template Transformation
    * Visual Studio SDK
    * (optional) [T4 Toolbox](https://marketplace.visualstudio.com/items?itemName=OlegVSych.T4Toolbox)

### Setup

Clone the repository [https://github.com/veler/BaZic](https://github.com/veler/BaZic).

Open the file [BaZic.sln](/BaZic.sln) in Visual Studio.

## Build

Several projects can be chosen as a startup project.
* **Sources/BaZic.Sample** : This is the main application. By running the project, an application designed to try BaZic code show up.

To build the solution, it is recommended to keep the `Any CPU` mode enabled.

## Test

Press `F5` to run the startup project, or, in Visual Studio, go to `Test, Windows, Test Explorer` to run all the unit tests.

# Third party libraries

* [Roslyn](https://github.com/dotnet/roslyn) ([Apache License 2.0](https://tldrlegal.com/license/apache-license-2.0-(apache-2.0)))

# Contributing to BaZic

Contributions to BaZic are welcomed in the form of construcutive, reproducible bug reports, feature requests that align to the project's goals, or better still a PR that's accompanied with passing tests.

If you have general questions or feedback about using BaZic, PLEASE DON'T CREATE AN ISSUE AND CONTACT THE [AUTHOR](http://www.velersoftware.com/) ON HIS WEBSITE OR POST TO STACKOVERFLOW.

## Bug Reports

If you're reporting a bug, please include a clear description of the issue, the version of BaZic you're using, and a set of clear repro steps.

Please remember that BaZic is a free and open-source project provided to the community with zero financial gain to the author(s). Any issues deemed to have a negative or arrogant tone will be closed without response.

## Feature Requests

The BaZic language and its component are subject to evoluate and you can contribute to it. However, any new feature request, including performance improvment, must be discussed at least at a design-level in an Issue.

# Licenses

[MIT License](/LICENSE.md)
