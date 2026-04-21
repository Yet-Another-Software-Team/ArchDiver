using System;
using System.IO;
using ArchDiver.Core.Pipeline;

namespace ArchDiver.Cli;

public class DirectoryExplorer
{
    private readonly PipelineControlUnit _pipeline;
    private readonly int _maxDepth;
    private readonly string _outputRoot;

    public DirectoryExplorer(PipelineControlUnit pipeline, string outputRoot, int maxDepth)
    {
        _pipeline = pipeline;
        _outputRoot = outputRoot;
        _maxDepth = maxDepth;
    }

    public void Explore(string rootPath, string currentPath, int depth)
    {
        if (depth > _maxDepth) return;
        if (currentPath.Replace("\\", "/").Contains("/.archdiver/out")) return;

        try
        {
            foreach (var file in Directory.GetFiles(currentPath))
            {
                ProcessFile(rootPath, file);
            }

            foreach (var dir in Directory.GetDirectories(currentPath))
            {
                Explore(rootPath, dir, depth + 1);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to access {currentPath}: {ex.Message}");
        }
    }

    private void ProcessFile(string rootPath, string filePath)
    {
        try
        {
            string sourceCode = File.ReadAllText(filePath);
            string relativePath = Path.GetRelativePath(rootPath, filePath);

            string? directoryName = Path.GetDirectoryName(relativePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(relativePath);
            string fileOutputDir = Path.Combine(_outputRoot, directoryName ?? "", fileNameWithoutExtension);

            _pipeline.ProcessAndExport(sourceCode, filePath, fileOutputDir);
        }
        catch (NotSupportedException) { }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing {filePath}: {ex.Message}");
        }
    }
}
