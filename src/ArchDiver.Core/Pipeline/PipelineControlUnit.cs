using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Infrastructure;
using ArchDiver.Core.Parsing;
using ArchDiver.Core.Storage;
using ArchDiver.Core.Models;

namespace ArchDiver.Core.Pipeline;

/// <summary>
/// Acts as the central controller and brain of the software, orchestrating the data pipeline.
/// </summary>
public class PipelineControlUnit
{
    private readonly ContextStorage _contextStorage;
    private readonly IArchLogger _logger;
    private readonly ILanguageRegistry _languageRegistry;

    public PipelineControlUnit(ILanguageRegistry languageRegistry, ContextStorage? contextStorage = null, IArchLogger? logger = null)
    {
        _languageRegistry = languageRegistry ?? throw new ArgumentNullException(nameof(languageRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _contextStorage = contextStorage ?? new ContextStorage(_logger);
    }

    /// <summary>
    /// Processes source code and returns the extracted semantic concepts.
    /// </summary>
    public FileAnalysisResult Process(string sourceCode, string filePath)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
            throw new ArgumentException("Source code cannot be null or whitespace.", nameof(sourceCode));

        var provider = _languageRegistry.Identify(filePath, sourceCode)
                       ?? throw new NotSupportedException($"No provider found for file: {filePath}");

        _logger.LogInfo($"PipelineControlUnit: Processing {filePath} ({provider.LanguageId})...");

        // 1. Parse AST
        var parser = new CodeParser(provider);
        var ast = parser.Parse(sourceCode);
        _contextStorage.StoreAst(ast);

        // 2. Extract Concepts
        var extractor = new ConceptExtractor(provider);
        return extractor.Extract(ast);
    }
}
