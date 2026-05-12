using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileSystemGlobbing;
using ArchDiver.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ArchDiver.Core.Pipeline;

/// <summary>
/// Explores a directory recursively and triggers the analysis pipeline for each file.
/// </summary>
public partial class DirectoryExplorer
{
    private readonly PipelineControlUnit _pipeline;
    private readonly ILogger<DirectoryExplorer> _logger;
    private readonly int _maxDepth;
    private readonly Matcher _ignoreMatcher;
    private readonly Action<string, FileAnalysisResult>? _onFileProcessed;
    private readonly ConcurrentBag<(string FilePath, Exception Exception)> _errors = new();

    private static void LogSkippingIgnoredDirectory(ILogger logger, string relativePath)
        => logger.LogDebug("Skipping ignored directory: {RelativePath}", relativePath);

    private static void LogSkippingIgnoredFile(ILogger logger, string relativeFilePath)
        => logger.LogDebug("Skipping ignored file: {RelativeFilePath}", relativeFilePath);

    private static void LogAccessFailed(ILogger logger, Exception ex, string currentPath)
        => logger.LogWarning(ex, "Failed to access {CurrentPath}", currentPath);

    private static void LogProcessingError(ILogger logger, Exception ex, string filePath)
        => logger.LogError(ex, "Error processing {FilePath}", filePath);

    public IEnumerable<(string FilePath, Exception Exception)> Errors => _errors;

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
    public void Explore(string rootPath, Action<int, int>? progressCallback = null)
    {
        _errors.Clear();
        var filesToProcess = new List<string>();
        CollectFiles(rootPath, rootPath, 0, filesToProcess);

        int totalFiles = filesToProcess.Count;
        int processedFiles = 0;

        Parallel.ForEach(filesToProcess, file =>
        {
            ProcessFile(rootPath, file);
            int current = Interlocked.Increment(ref processedFiles);
            progressCallback?.Invoke(current, totalFiles);
        });
    }

    private void CollectFiles(string rootPath, string currentPath, int depth, List<string> filesToProcess)
    {
        if (depth > _maxDepth) return;

        string relativePath = Path.GetRelativePath(rootPath, currentPath);

        // If it's not the root itself, check if this directory is ignored
        if (relativePath != "." && IsIgnored(relativePath))
        {
            LogSkippingIgnoredDirectory(_logger, relativePath);
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
                    LogSkippingIgnoredFile(_logger, relativeFilePath);
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
            LogAccessFailed(_logger, ex, currentPath);
        }
    }

    private bool IsIgnored(string relativePath)
    {
        string normalizedPath = relativePath.Replace("\\", "/");

        return _ignoreMatcher.Match(normalizedPath).HasMatches;
    }

    private void ProcessFile(string rootPath, string filePath)
    {
        try
        {
            string sourceCode = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                return;
            }
            var result = _pipeline.CreateIR(sourceCode, filePath);

            _onFileProcessed?.Invoke(filePath, result);
        }
        catch (NotSupportedException)
        {
        }
        catch (Exception ex)
        {
            _errors.Add((filePath, ex));
            LogProcessingError(_logger, ex, filePath);
        }
    }
}
