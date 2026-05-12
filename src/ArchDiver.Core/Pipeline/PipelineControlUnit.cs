using Microsoft.Extensions.Logging;
using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Storage;
using ArchDiver.Core.Models;
using ArchDiver.GraphConstruction;
using ArchDiver.Shared.Models;
using ArchDiver.SmellAnalyzer;

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

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Analyzing {FilePath}...", filePath);
        }

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

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Constructing code graph from {RootDirectory}...", analysisArtifactsRootDirectory);
        }

        var graph = _graphBuilder.BuildCodeGraph(analysisArtifactsRootDirectory);
        _contextStorage.StoreGraph(graph);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "Code graph stored in ContextStorage (Nodes: {NodeCount}, Edges: {EdgeCount}).",
                graph.Nodes.Count,
                graph.Edges.Count);
        }

        return graph;
    }

    /// <summary>
    /// Analyzes the stored code graph for architectural smells using the specified ONNX model.
    /// </summary>
    public Dictionary<int, float> AnalyzeSmells(string? modelPath = null)
    {
        var graph = _contextStorage.CodeGraph;
        if (graph == null)
            throw new InvalidOperationException("Code graph is not available. Ensure BuildAndStoreGraph has been called.");

        if (modelPath != null)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Running smell detection using model: {ModelPath}", modelPath);
            }
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Running smell detection using built-in model");
            }
        }

        using var detector = new SmellDetector(modelPath);
        var predictions = detector.AnalyzeGraph(graph);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Raw prediction results:");
            foreach (var kvp in predictions)
            {
                _logger.LogDebug("  Node {NodeId}: {Score}", kvp.Key, kvp.Value);
            }
        }

        return predictions;
    }
}
