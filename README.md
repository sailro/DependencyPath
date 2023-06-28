# DependencyPath
[![NuGet](https://img.shields.io/nuget/v/DependencyPath.svg)](https://www.nuget.org/packages/DependencyPath/)
Find transitive dependencies in assemblies.

## Installation

You can easily install as a global dotnet tool:
```
dotnet tool install --global DependencyPath
```
You can then invoke the tool using the following command: `dependency-path`.

## Usage
```
USAGE:
    DependencyPath.dll <assemblies> <dependency> [OPTIONS]

ARGUMENTS:
    <assemblies>    Assemblies
    <dependency>    Dependency to search

OPTIONS:
    -h,  --help           Prints help information
    -v,  --version        Display resolved versions
    -a,  --version-all    Display expected and resolved versions
    -t,  --token          Skip public key token
    -r,  --recurse        Recurse sub-directories
         --verbose        Verbose
    -d,  --depth          Max search depth
    -p,  --path           Assembly search path

COMMANDS:
    scan <assemblies> <dependency>    Scan assemblies
```

## Demo

```
dependency-path SyntaxTree*.dll NewtonSoft.Json --depth 3 -t b77a5c561934e089 -t cc7b13ffcd2ddd51

SyntaxTree.VisualStudio.Unity.CodeLens -> Microsoft.VisualStudio.Language -> Newtonsoft.Json
SyntaxTree.VisualStudio.Unity -> Newtonsoft.Json
SyntaxTree.VisualStudio.Unity -> Microsoft.VisualStudio.LanguageServices -> Newtonsoft.Json
SyntaxTree.VisualStudio.Unity -> Microsoft.VisualStudio.Telemetry -> Newtonsoft.Json
SyntaxTree.VisualStudio.Unity -> Microsoft.VisualStudio.Language -> Newtonsoft.Json
SyntaxTree.VisualStudio.Unity -> Microsoft.VisualStudio.Utilities -> Newtonsoft.Json
SyntaxTree.VisualStudio.Unity.Tests -> SyntaxTree.VisualStudio.Unity -> Newtonsoft.Json
```
