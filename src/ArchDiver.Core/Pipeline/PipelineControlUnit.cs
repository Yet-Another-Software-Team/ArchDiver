using Microsoft.Extensions.Logging;
using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Storage;
using ArchDiver.Core.Models;
using ArchDiver.GraphConstruction;
using ArchDiver.Shared.Models;
using ArchDiver.SmellAnalyzer;

namespace ArchDiver.Core.Pipeline;

/// <summary>
/// Orchestrates the data analysis pipeline.
/// </summary>
public partial class PipelineControlUnit(
    ICodeAnalysisEngine analysisEngine,
    ILogger<PipelineControlUnit> logger,
    ContextStorage? contextStorage = null,
    GraphBuilder? graphBuilder = null)
{
    private readonly ContextStorage _contextStorage = contextStorage ?? new ContextStorage();
    private readonly ILogger<PipelineControlUnit> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICodeAnalysisEngine _analysisEngine = analysisEngine ?? throw new ArgumentNullException(nameof(analysisEngine));
    private readonly GraphBuilder _graphBuilder = graphBuilder ?? new GraphBuilder();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Analyzing {FilePath}...")]
    static partial void LogAnalyzingFile(ILogger logger, string filePath);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Constructing code graph from {RootDirectory}...")]
    static partial void LogConstructingGraph(ILogger logger, string rootDirectory);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Code graph stored in ContextStorage (Nodes: {NodeCount}, Edges: {EdgeCount}).")]
    static partial void LogGraphStored(ILogger logger, int nodeCount, int edgeCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Running smell detection using model: {ModelPath}")]
    static partial void LogRunningSmellDetectionWithModel(ILogger logger, string modelPath);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Running smell detection using built-in model")]
    static partial void LogRunningSmellDetectionBuiltIn(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Raw prediction results:")]
    static partial void LogRawPredictionsHeader(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "  Node {NodeId}: {Score}")]
    static partial void LogNodePrediction(ILogger logger, int nodeId, float score);

    public ContextStorage ContextStorage => _contextStorage;

    /// <summary>
    /// Processes source code and returns the extracted semantic concepts.
    /// </summary>
    public FileAnalysisResult CreateIR(string sourceCode, string filePath)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
            throw new ArgumentException("Source code cannot be null or whitespace.", nameof(sourceCode));

        LogAnalyzingFile(_logger, filePath);

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

        LogConstructingGraph(_logger, analysisArtifactsRootDirectory);

        var graph = _graphBuilder.BuildCodeGraph(analysisArtifactsRootDirectory);
        _contextStorage.StoreGraph(graph);

        LogGraphStored(_logger, graph.Nodes.Count, graph.Edges.Count);

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
            LogRunningSmellDetectionWithModel(_logger, modelPath);
        }
        else
        {
            LogRunningSmellDetectionBuiltIn(_logger);
        }

        using var detector = new SmellDetector(modelPath);
        var predictions = detector.AnalyzeGraph(graph);

        LogRawPredictionsHeader(_logger);
        foreach (var kvp in predictions)
        {
            LogNodePrediction(_logger, kvp.Key, kvp.Value);
        }

        return predictions;
    }
}
