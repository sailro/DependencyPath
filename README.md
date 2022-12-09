# DependencyPath
Find transitive dependencies in assemblies

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
    -h, --help       Prints help information
    -v, --version    Display versions
    -t, --token      Skip public key token
    -r, --recurse    Recurse sub-directories
        --verbose    Verbose
    -d, --depth      Max search depth

COMMANDS:
    scan <assemblies> <dependency>    Scan assemblies
```

## Demo

```
dependency-path SyntaxTree*.dll NewtonSoft.Json --depth 3 --version

SyntaxTree.VisualStudio.Unity.CodeLens (17.5.0.0) -> Microsoft.VisualStudio.Language (17.0.0.0) -> Newtonsoft.Json (13.0.0.0)
SyntaxTree.VisualStudio.Unity (17.5.0.0) -> Newtonsoft.Json (13.0.0.0)
SyntaxTree.VisualStudio.Unity (17.5.0.0) -> Microsoft.VisualStudio.LanguageServices (4.3.0.0) -> Newtonsoft.Json (13.0.0.0)
SyntaxTree.VisualStudio.Unity (17.5.0.0) -> Microsoft.VisualStudio.Telemetry (16.0.0.0) -> Newtonsoft.Json (13.0.0.0)
SyntaxTree.VisualStudio.Unity (17.5.0.0) -> Microsoft.VisualStudio.Language (17.0.0.0) -> Newtonsoft.Json (13.0.0.0)
SyntaxTree.VisualStudio.Unity (17.5.0.0) -> Microsoft.VisualStudio.Utilities (17.0.0.0) -> Newtonsoft.Json (13.0.0.0)
SyntaxTree.VisualStudio.Unity.Tests (0.0.0.0) -> SyntaxTree.VisualStudio.Unity (17.5.0.0) -> Newtonsoft.Json (13.0.0.0)
```
