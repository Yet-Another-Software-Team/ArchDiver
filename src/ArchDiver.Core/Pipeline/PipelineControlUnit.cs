using Microsoft.Extensions.Logging;
using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Storage;
using ArchDiver.Core.Models;

namespace ArchDiver.Core.Pipeline;

/// <summary>
/// Acts as the central controller and brain of the software, orchestrating the data pipeline.
/// </summary>
public class PipelineControlUnit(
    ICodeAnalysisEngine analysisEngine,
    ILogger<PipelineControlUnit> logger,
    ContextStorage? contextStorage = null)
{
    private readonly ContextStorage _contextStorage = contextStorage ?? new ContextStorage();
    private readonly ILogger<PipelineControlUnit> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICodeAnalysisEngine _analysisEngine = analysisEngine ?? throw new ArgumentNullException(nameof(analysisEngine));

    /// <summary>
    /// Processes source code and returns the extracted semantic concepts.
    /// </summary>
    public FileAnalysisResult Process(string sourceCode, string filePath)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
            throw new ArgumentException("Source code cannot be null or whitespace.", nameof(sourceCode));

        _logger.LogInformation("Analyzing {FilePath}...", filePath);

        var result = _analysisEngine.Analyze(sourceCode, filePath);
        return result;
    }
}
