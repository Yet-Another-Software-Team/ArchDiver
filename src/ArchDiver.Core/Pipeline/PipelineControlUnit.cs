using Microsoft.Extensions.Logging;
using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Storage;
using ArchDiver.Core.Models;
using ArchDiver.GraphConstruction;
using ArchDiver.Shared.Models;
using ArchDiver.SmellAnalyzer;
using System;
using System.Collections.Generic;

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

    private static void LogAnalyzingFile(ILogger logger, string filePath)
        => logger.LogDebug("Analyzing {FilePath}...", filePath);

    private static void LogConstructingGraph(ILogger logger, string rootDirectory)
        => logger.LogDebug("Constructing code graph from {RootDirectory}...", rootDirectory);

    private static void LogGraphStored(ILogger logger, int nodeCount, int edgeCount)
        => logger.LogDebug("Code graph stored in ContextStorage (Nodes: {NodeCount}, Edges: {EdgeCount}).", nodeCount, edgeCount);

    private static void LogRunningSmellDetectionWithModel(ILogger logger, string modelPath)
        => logger.LogDebug("Running smell detection using model: {ModelPath}", modelPath);

    private static void LogRunningSmellDetectionBuiltIn(ILogger logger)
        => logger.LogDebug("Running smell detection using built-in model");

    private static void LogRawPredictionsHeader(ILogger logger)
        => logger.LogDebug("Raw prediction results:");

    private static void LogNodePrediction(ILogger logger, int nodeId, float score)
        => logger.LogDebug("  Node {NodeId}: {Score}", nodeId, score);

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
