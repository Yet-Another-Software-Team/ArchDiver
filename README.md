# ArchDiver

ArchDiver is a high-performance software architecture analysis tool designed to dive deep into codebases, extract structural relationships, and generate architectural insights. Built on top of **.NET 10.0** and **Tree-sitter**, it provides a robust pipeline for parsing, analyzing, and visualizing complex software systems.

## 🚀 Features

- **Multi-Language Parsing**: Powered by Tree-sitter. ArchDiver uses a **microkernel architecture** where each language is implemented as an independent provider.
- **Automatic Language Detection**: CLI automatically detects the language based on file extension.
- **Rich AST Model**: Captures precise source ranges, parent-child relationships, and metadata.
- **Native Query Support**: Execute Tree-sitter S-expression queries directly against source files via CLI.
- **Cross-Platform**: Built on .NET 10.0, ArchDiver runs on **Windows, Linux, and macOS**.

## 🛠 Project Structure

- `src/ArchDiver.Core`: The kernel. Contains the registry, bootstrapper, and base AST model.
- `src/ArchDiver.Core/Languages`: Language-specific providers (e.g., `CSharpLanguageProvider.cs`).
- `src/ArchDiver.Cli`: Command-line interface with automatic language detection.
- `src/ArchDiver.Tests`: Validation suite.

## 🏁 Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Build & Test

```powershell
# Build the solution
dotnet build ArchDiver.slnx

# Run the unit tests
dotnet test ArchDiver.slnx
```

## 💻 CLI Usage

The ArchDiver CLI automatically detects the language based on the file extension.

### Parse and Inspect AST
```powershell
dotnet run --project src/ArchDiver.Cli -- parse <file_path> [--raw]
```
Use `--raw` to see native Tree-sitter field names (useful for writing queries).

### Execute Tree-sitter Queries
```powershell
dotnet run --project src/ArchDiver.Cli -- query <file_path> "<query_string>"
```
**Example**: Find all property names in a C# file (no `--lang` needed):
```powershell
dotnet run --project src/ArchDiver.Cli -- query src/ArchDiver.Core/CodeParser.cs "(property_declaration name: (identifier) @name)"
```


## 🌍 Supported Languages

ArchDiver currently supports the following languages out-of-the-box:
- **C#** (`.cs`)
- **Python** (`.py`)
- **Java** (`.java`)

More languages (Go, JavaScript, C++, etc.) can be easily added by implementing a new `ILanguageProvider`.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
