# ArchDiver

ArchDiver is a high-performance software architecture analysis tool designed to dive deep into codebases, extract structural relationships, and generate architectural insights. Built on top of **.NET 10.0** and **Tree-sitter**, it provides a robust pipeline for parsing, analyzing, and visualizing complex software systems.

## 🚀 Features

- **Multi-Language Parsing**: Powered by Tree-sitter for fast, incremental, and accurate Abstract Syntax Tree (AST) generation.
- **Rich AST Model**: Captures precise source ranges, parent-child relationships, and metadata essential for architectural mapping.
- **Native Query Support**: Execute Tree-sitter S-expression queries directly against source files via CLI.
- **Extensible Pipeline**: Orchestrated by a central Control Unit, allowing for modular analysis stages.

## 🛠 Project Structure

- `src/ArchDiver.Core`: The engine room. Contains the parser, context storage, and pipeline orchestration.
- `src/ArchDiver.Cli`: Command-line interface for interactive AST inspection and querying.
- `src/ArchDiver.Tests`: Comprehensive validation suite ensuring parsing accuracy.
- `ArchDiver.slnx`: Modern XML-based solution file.

## 🏁 Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows (Native Tree-sitter binaries included for win-x64, win-arm64, win-x86).

### Build & Test

```powershell
# Build the solution
dotnet build ArchDiver.slnx

# Run the unit tests
dotnet test ArchDiver.slnx
```

## 💻 CLI Usage

The ArchDiver CLI allows you to inspect the structure of your code and run powerful queries.

### Parse and Inspect AST
To visualize the full tree structure of a file:
```powershell
dotnet run --project src/ArchDiver.Cli -- parse <file_path> [--lang <language>]
```

### Execute Tree-sitter Queries
To find specific patterns using S-expressions:
```powershell
dotnet run --project src/ArchDiver.Cli -- query <file_path> "<query_string>" [--lang <language>]
```
**Example**: Find all property names in a C# file:
```powershell
dotnet run --project src/ArchDiver.Cli -- query src/ArchDiver.Core/CodeParser.cs "(property_declaration name: (identifier) @name)"
```


## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
