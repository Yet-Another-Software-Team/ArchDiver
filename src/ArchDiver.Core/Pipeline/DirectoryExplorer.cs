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
        var filesToProcess = new List<string>();
        CollectFiles(rootPath, rootPath, 0, filesToProcess);

        Parallel.ForEach(filesToProcess, file =>
        {
            ProcessFile(rootPath, file);
        });
    }

    private void CollectFiles(string rootPath, string currentPath, int depth, List<string> filesToProcess)
    {
        if (depth > _maxDepth) return;

        string relativePath = Path.GetRelativePath(rootPath, currentPath);

        // If it's not the root itself, check if this directory is ignored
        if (relativePath != "." && IsIgnored(relativePath))
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Skipping ignored directory: {RelativePath}", relativePath);
            }
            return;
        }

        try
        {
            var files = Directory.GetFiles(currentPath);
            foreach (var file in files)
            {
                string relativeFilePath = Path.GetRelativePath(rootPath, file);
                if (IsIgnored(relativeFilePath))
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Skipping ignored file: {RelativeFilePath}", relativeFilePath);
                    }
                    continue;
                }

                filesToProcess.Add(file);
            }

            foreach (var dir in Directory.GetDirectories(currentPath))
            {
                CollectFiles(rootPath, dir, depth + 1, filesToProcess);
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
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                // Skip empty files
                return;
            }
            var result = _pipeline.CreateIR(sourceCode, filePath);

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
