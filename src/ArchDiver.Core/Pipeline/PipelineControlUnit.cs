using ArchDiver.Core.Abstractions;
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
    private readonly ICodeAnalysisEngine _analysisEngine;

    public PipelineControlUnit(ICodeAnalysisEngine analysisEngine, ContextStorage? contextStorage = null, IArchLogger? logger = null)
    {
        _analysisEngine = analysisEngine ?? throw new ArgumentNullException(nameof(analysisEngine));
        _contextStorage = contextStorage ?? new ContextStorage(_logger);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes source code and returns the extracted semantic concepts.
    /// </summary>
    public FileAnalysisResult Process(string sourceCode, string filePath)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
            throw new ArgumentException("Source code cannot be null or whitespace.", nameof(sourceCode));

        _logger.LogInfo($"PipelineControlUnit: Analyzing {filePath}...");

        var result = _analysisEngine.Analyze(sourceCode, filePath);
        return result;
    }
}
