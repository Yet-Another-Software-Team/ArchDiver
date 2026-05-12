# ArchDiver

ArchDiver is an AI-driven codebase analysis tool designed to detect architectural smells, such as feature concentration. By leveraging Tree-sitter to parse code into a language-independent Intermediate Representation (IR), ArchDiver utilizes Graph Neural Networks to analyze structural dependencies.

## Key Features

- **Interactive CLI:** Real-time feedback with status spinners and progress indicators.
- **AI-Powered Analysis:** Detects architectural smells using embedded ONNX models.
- **True Single-File Distribution:** Fully self-contained executable for easy deployment.
- **Multi-Language Support:** C#, Java, and Python support via Tree-sitter.
- **Robust Error Reporting:** Detailed summaries of analysis failures with troubleshooting tips.
- **Diagnostic Logging:** Automatic file logging for detailed analysis traces.

## Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Building from Source
To build the entire solution and run tests:
```bash
# Clone the repository
git clone https://github.com/Yet-Another-Software-Team/ArchDiver.git
cd ArchDiver

# Build the solution
dotnet build ArchDiver.slnx

# Run tests
dotnet test ArchDiver.slnx
```

### Packaging (True Single-File)
To create a fully self-contained, single-file executable for Windows (includes runtime and all native dependencies):
```bash
dotnet publish src/ArchDiver.Cli/ArchDiver.Cli.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish
```
The resulting `ArchDiver.Cli.exe` in the `./publish` directory is the only file needed for distribution.

## CLI Reference

### Smell Analysis
The `analyze` command runs the full pipeline—exploration, graph construction, and AI-driven smell detection.

```bash
archdiver analyze <directory_path> [model_path] [--show-all] [--max-depth <n>]
```
- `<directory_path>`: The root directory of the project to analyze.
- `[model_path]`: (Optional) Path to a custom ONNX model. Defaults to the built-in model.
- `--show-all`: Displays all predictions, even those below the confidence threshold.
- `--max-depth`: Maximum directory recursion depth (default: 10).

### Directory Exploration
The `explore` command recursively scans a directory and extracts semantic concepts to TOML artifacts.

```bash
archdiver explore <directory_path> [--max-depth <n>]
```

### Configuration
Manage your `archdiver.toml` configuration.
```bash
archdiver config        # Show current config
archdiver config create # Create default config
```

## Supported Languages

| Language | Extension | Provider |
|----------|-----------|----------|
| C#       | `.cs`      | `CSharpLanguageProvider` |
| Python   | `.py`      | `PythonLanguageProvider` |
| Java     | `.java`    | `JavaLanguageProvider` |

## Project Structure

- `src/ArchDiver.Core`: Core framework containing the microkernel and extraction pipeline.
- `src/ArchDiver.Cli`: Command-line entry point.
- `src/ArchDiver.Parser`: Tree-sitter based parsing and concept extraction.
- `src/ArchDiver.SmellAnalyzer`: ONNX-based architectural smell detection.
- `src/ArchDiver.Shared`: Shared models and graph definitions.

## License

MIT License - see the [LICENSE](LICENSE) file for details.
