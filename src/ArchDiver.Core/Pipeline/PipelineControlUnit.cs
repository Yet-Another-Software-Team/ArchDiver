using ArchDiver.Core.Infrastructure;
using ArchDiver.Core.Parsing;
using ArchDiver.Core.Storage;

namespace ArchDiver.Core.Pipeline;

/// <summary>
/// Acts as the central controller and brain of the software, orchestrating the data pipeline.
/// </summary>
public class PipelineControlUnit
{
    private readonly ContextStorage _contextStorage;

    public PipelineControlUnit(ContextStorage? contextStorage = null)
    {
        _contextStorage = contextStorage ?? new ContextStorage();
    }

    /// <summary>
    /// Processes the given source code through the pipeline.
    /// Currently only parses the code into an Abstract Syntax Tree (AST).
    /// </summary>
    /// <param name="sourceCode">The source code to process.</param>
    /// <param name="languageId">The language ID to use for parsing.</param>
    /// <returns>The root node of the generated AST.</returns>
    /// <summary>
    /// Processes source code and exports the extracted semantic concepts to TOML files (one per component).
    /// </summary>
    public void ProcessAndExport(string sourceCode, string filePath, string outputDir)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
            throw new ArgumentException("Source code cannot be null or whitespace.", nameof(sourceCode));

        var provider = LanguageRegistry.Identify(filePath, sourceCode)
                       ?? throw new NotSupportedException($"No provider found for file: {filePath}");

        Console.WriteLine($"PipelineControlUnit: Processing {filePath} ({provider.LanguageId})...");

        // 1. Parse AST
        var parser = new CodeParser(provider);
        var ast = parser.Parse(sourceCode);
        _contextStorage.StoreAst(ast);

        // 2. Extract Concepts
        var extractor = new ConceptExtractor(provider);
        var result = extractor.Extract(ast);

        // 3. Export to TOML (one file per component)
        foreach (var comp in result.Components)
        {
            string componentPath = Path.Combine(outputDir, $"{comp.Name}.toml");
            TomlExporter.ExportComponent(comp, result.Imports, componentPath);
        }
    }
}
