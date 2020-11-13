# C++ Unit Test Generator

## Features

- Identify external headers required by class under test
- Add Header guards to external headers if missing
- Identify functions inside class under test
- Generate sample tests for all functions

## Project Frameworks

- [.Net Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)

## IDE

* Microsoft Visual Studio Community 2019 Version 16.8.0

## Build Instructions

- Open and build `..\MuTest.sln` on Visual Studio

## Input
```bash
$ Enter Solution Path: <solution path containing class under test>
$ Enter Source Class Path to Test: <class under test path>
```

## Outcome
1. Dependent Header files with header guards 
2. Test Code for Class Under test
