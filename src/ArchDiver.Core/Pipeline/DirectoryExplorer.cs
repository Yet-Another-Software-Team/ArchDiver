using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Models;

namespace ArchDiver.Core.Pipeline;

/// <summary>
/// Orchestrates the recursive exploration of a directory, triggering the pipeline for each file.
/// </summary>
public class DirectoryExplorer
{
    private readonly PipelineControlUnit _pipeline;
    private readonly IArchLogger _logger;
    private readonly int _maxDepth;
    private readonly List<string> _ignorePatterns;
    private readonly Action<string, FileAnalysisResult>? _onFileProcessed;

    public DirectoryExplorer(
        PipelineControlUnit pipeline,
        IArchLogger logger,
        int maxDepth = 10,
        List<string>? ignorePatterns = null,
        Action<string, FileAnalysisResult>? onFileProcessed = null)
    {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxDepth = maxDepth;
        _ignorePatterns = ignorePatterns ?? new List<string>();
        _onFileProcessed = onFileProcessed;
    }

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
            _logger.LogWarning($"DirectoryExplorer: Failed to access {currentPath}: {ex.Message}");
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
        }
        catch (Exception ex)
        {
            _logger.LogError($"DirectoryExplorer: Error processing {filePath}: {ex.Message}");
        }
    }
}
