using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileSystemGlobbing;
using ArchDiver.Core.Models;

namespace ArchDiver.Core.Pipeline;

/// <summary>
/// Orchestrates the recursive exploration of a directory, triggering the pipeline for each file.
/// </summary>
public class DirectoryExplorer
{
    private readonly PipelineControlUnit _pipeline;
    private readonly ILogger<DirectoryExplorer> _logger;
    private readonly int _maxDepth;
    private readonly Matcher _ignoreMatcher;
    private readonly Action<string, FileAnalysisResult>? _onFileProcessed;

    public DirectoryExplorer(
        PipelineControlUnit pipeline,
        ILogger<DirectoryExplorer> logger,
        int maxDepth = 10,
        List<string>? ignorePatterns = null,
        Action<string, FileAnalysisResult>? onFileProcessed = null)
    {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxDepth = maxDepth;
        _onFileProcessed = onFileProcessed;

        _ignoreMatcher = new Matcher();
        if (ignorePatterns != null && ignorePatterns.Count > 0)
        {
            _ignoreMatcher.AddIncludePatterns(ignorePatterns);
        }
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

        string relativePath = Path.GetRelativePath(rootPath, currentPath);

        // If it's not the root itself, check if this directory is ignored
        if (relativePath != "." && IsIgnored(relativePath))
        {
            _logger.LogDebug("Skipping ignored directory: {RelativePath}", relativePath);
            return;
        }

        try
        {
            foreach (var file in Directory.GetFiles(currentPath))
            {
                string relativeFilePath = Path.GetRelativePath(rootPath, file);
                if (IsIgnored(relativeFilePath))
                {
                    _logger.LogDebug("Skipping ignored file: {RelativeFilePath}", relativeFilePath);
                    continue;
                }

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

    private bool IsIgnored(string relativePath)
    {
        // Microsoft.Extensions.FileSystemGlobbing uses '/' even on Windows
        string normalizedPath = relativePath.Replace("\\", "/");

        // Matcher.Match returns results if the path matches any of the patterns
        return _ignoreMatcher.Match(normalizedPath).HasMatches;
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
