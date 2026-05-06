using Microsoft.Extensions.Logging;
using ArchDiver.Core.Models;

namespace ArchDiver.Core.Pipeline;

/// <summary>
/// Orchestrates the recursive exploration of a directory, triggering the pipeline for each file.
/// </summary>
public class DirectoryExplorer(
    PipelineControlUnit pipeline,
    ILogger<DirectoryExplorer> logger,
    int maxDepth = 10,
    List<string>? ignorePatterns = null,
    Action<string, FileAnalysisResult>? onFileProcessed = null)
{
    private readonly PipelineControlUnit _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
    private readonly ILogger<DirectoryExplorer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly int _maxDepth = maxDepth;
    private readonly List<string> _ignorePatterns = ignorePatterns ?? new List<string>();
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

        string normalizedPath = currentPath.Replace("\\", "/");
        if (_ignorePatterns.Any(pattern => normalizedPath.Contains(pattern))) return;

        try
        {
            foreach (var file in Directory.GetFiles(currentPath))
            {
                if (_ignorePatterns.Any(pattern => file.Replace("\\", "/").Contains(pattern))) continue;
                ProcessFile(rootPath, file);
            }

            foreach (var dir in Directory.GetDirectories(currentPath))
            {
                ExploreInternal(rootPath, dir, depth + 1);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to access {CurrentPath}", currentPath);
        }
    }

    private void ProcessFile(string rootPath, string filePath)
    {
        try
        {
            string sourceCode = File.ReadAllText(filePath);
            var result = _pipeline.Process(sourceCode, filePath);

            _onFileProcessed?.Invoke(filePath, result);
        }
        catch (NotSupportedException)
        {
            // Silently skip
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing {FilePath}", filePath);
        }
    }
}
