using Microsoft.Extensions.Logging;
using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Storage;
using ArchDiver.Core.Models;
using ArchDiver.GraphConstruction;
using ArchDiver.Shared.Models;

namespace ArchDiver.Core.Pipeline;

/// <summary>
/// Acts as the central controller and brain of the software, orchestrating the data pipeline.
/// </summary>
public class PipelineControlUnit(
    ICodeAnalysisEngine analysisEngine,
    ILogger<PipelineControlUnit> logger,
    ContextStorage? contextStorage = null,
    GraphBuilder? graphBuilder = null)
{
    private readonly ContextStorage _contextStorage = contextStorage ?? new ContextStorage();
    private readonly ILogger<PipelineControlUnit> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICodeAnalysisEngine _analysisEngine = analysisEngine ?? throw new ArgumentNullException(nameof(analysisEngine));
    private readonly GraphBuilder _graphBuilder = graphBuilder ?? new GraphBuilder();

    public ContextStorage ContextStorage => _contextStorage;

    /// <summary>
    /// Processes source code and returns the extracted semantic concepts.
    /// </summary>
    public FileAnalysisResult CreateIR(string sourceCode, string filePath)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
            throw new ArgumentException("Source code cannot be null or whitespace.", nameof(sourceCode));

        _logger.LogInformation("Analyzing {FilePath}...", filePath);

        var result = _analysisEngine.Analyze(sourceCode, filePath);
        return result;
    }

    /// <summary>
    /// Constructs a code graph from the given directory (typically the analysis output directory)
    /// and stores it in <see cref="ContextStorage"/>.
    /// </summary>
    public Graph BuildAndStoreGraph(string analysisArtifactsRootDirectory)
    {
        if (string.IsNullOrWhiteSpace(analysisArtifactsRootDirectory))
            throw new ArgumentException("Directory cannot be null or whitespace.", nameof(analysisArtifactsRootDirectory));

        _logger.LogInformation("Constructing code graph from {RootDirectory}...", analysisArtifactsRootDirectory);

        var graph = _graphBuilder.BuildCodeGraph(analysisArtifactsRootDirectory);
        _contextStorage.StoreGraph(graph);

        _logger.LogInformation(
            "Code graph stored in ContextStorage (Nodes: {NodeCount}, Edges: {EdgeCount}).",
            graph.Nodes.Count,
            graph.Edges.Count);

        return graph;
    }
}
