# ArchDiver

ArchDiver is an AI-driven codebase analysis tool designed to detect architectural smells, such as feature concentration. By leveraging Tree-sitter to parse code into a language-independent Intermediate Representation (IR), ArchDiver utilizes Graph Neural Networks to analyze structural dependencies. It operates as a Model Context Protocol (MCP) server, allowing external AI agents to seamlessly access and interpret your software's architecture.

## Project Structure

- `src/ArchDiver.Core`: Core framework containing the microkernel, AST models, and the extraction pipeline.
- `src/ArchDiver.Core/Abstractions`: Definition of provider interfaces and semantic attributes.
- `src/ArchDiver.Core/Languages`: Language-specific implementation providers (CSharp, Python, Java).
- `src/ArchDiver.Cli`: Command-line entry point for executing analysis pipelines.
- `src/ArchDiver.Tests`: Unit testing suite for parser and extractor validation.

## Technical Specifications

### Prerequisites
- .NET 10.0 SDK

### Build and Test
```bash
# Build solution
dotnet build ArchDiver.slnx

# Execute tests
dotnet test ArchDiver.slnx
```

## CLI Reference

### Directory Exploration
The `explore` command recursively scans a directory, identifies supported source files, extracts semantic concepts, and persists the results as TOML files in the `.archdiver/out` directory.

```bash
dotnet run --project src/ArchDiver.Cli -- explore <directory_path> [--max-depth <n>]
```

## Supported Languages

| Language | Extension | Provider |
|----------|-----------|----------|
| C#       | `.cs`      | `CSharpLanguageProvider` |
| Python   | `.py`      | `PythonLanguageProvider` |
| Java     | `.java`    | `JavaLanguageProvider` |

Extending language support requires implementing `ILanguageProvider` (or inheriting from `LanguageProviderBase`) and defining appropriate `NodeBinding` attributes for the target grammar.

## License

MIT License - see the [LICENSE](LICENSE) file for details.
