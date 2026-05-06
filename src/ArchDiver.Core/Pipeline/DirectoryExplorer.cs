using ArchDiver.Core.Pipeline;
using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Models;

namespace ArchDiver.Core.Pipeline;

/// <summary>
/// Orchestrates the recursive exploration of a directory, triggering the pipeline for each file.
/// </summary>
public class DirectoryExplorer(
    PipelineControlUnit pipeline,
    IArchLogger logger,
    int maxDepth = 10,
    Action<string, FileAnalysisResult>? onFileProcessed = null)
{
    private readonly PipelineControlUnit _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
    private readonly IArchLogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly int _maxDepth = maxDepth;
    private readonly Action<string, FileAnalysisResult>? _onFileProcessed = onFileProcessed;

    /// <summary>
    /// Recursively explores the directory starting from rootPath.
    /// </summary>
    public void Explore(string rootPath)
    {
        ExploreInternal(rootPath, rootPath, 0);
    }

    private void ExploreInternal(string rootPath, string currentPath, int depth)
    {
        if (depth > _maxDepth) return;

        // Skip internal ArchDiver output directories
        if (currentPath.Replace("\\", "/").Contains("/.archdiver")) return;

        try
        {
            foreach (var file in Directory.GetFiles(currentPath))
            {
                ProcessFile(rootPath, file);
            }

            foreach (var dir in Directory.GetDirectories(currentPath))
            {
                ExploreInternal(rootPath, dir, depth + 1);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"DirectoryExplorer: Failed to access {currentPath}: {ex.Message}");
        }
    }

    private void ProcessFile(string rootPath, string filePath)
    {
        try
        {
            string sourceCode = File.ReadAllText(filePath);
            var result = _pipeline.Process(sourceCode, filePath);

            // Callback for handling results (e.g., exporting to disk, or building a graph)
            _onFileProcessed?.Invoke(filePath, result);
        }
        catch (NotSupportedException)
        {
            // Silently skip files that are not supported by the analysis engine
            _logger.LogInfo($"DirectoryExplorer: Skipped {filePath} (not supported)");
        }
        catch (Exception ex)
        {
            _logger.LogError($"DirectoryExplorer: Error processing {filePath}: {ex.Message}");
        }
    }
}
