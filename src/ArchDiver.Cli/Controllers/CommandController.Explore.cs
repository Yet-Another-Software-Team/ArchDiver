using ArchDiver.Core.Models;
using ArchDiver.Core.Infrastructure;
using ArchDiver.Core.Pipeline;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ArchDiver.Cli.Controllers;

public partial class CommandController
{
    private int HandleExplore(string[] args, ProjectConfig config)
    {
        if (args.Length < 2)
        {
            LogMissingDirectory(_logger);
            return 1;
        }

        string rootPath = Path.GetFullPath(args[1]);
        if (!Directory.Exists(rootPath))
        {
            LogDirectoryNotFound(_logger, rootPath);
            return 1;
        }

        int maxDepth = config.Analysis.MaxDepth;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--max-depth" && i + 1 < args.Length && int.TryParse(args[i + 1], out int depth))
            {
                maxDepth = depth;
            }
        }

        string outputRoot = Path.Combine(rootPath, _outputDir);
        if (Directory.Exists(outputRoot)) Directory.Delete(outputRoot, true);
        Directory.CreateDirectory(outputRoot);

        LogExploring(_logger, rootPath, maxDepth);

        var pipelineLogger = _loggerFactory.CreateLogger<PipelineControlUnit>();
        var explorerLogger = _loggerFactory.CreateLogger<DirectoryExplorer>();

        var pipeline = new PipelineControlUnit(_analysisEngine, pipelineLogger);
        var exporter = new TomlExporter();

        var explorer = new DirectoryExplorer(pipeline, explorerLogger, maxDepth, config.Analysis.IgnorePatterns, (filePath, result) =>
        {
            string relativePath = Path.GetRelativePath(rootPath, filePath);
            string? directoryName = Path.GetDirectoryName(relativePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(relativePath);

            string fileOutputDir = Path.Combine(outputRoot, directoryName ?? "");

            exporter.Export(result, fileOutputDir, fileNameWithoutExtension);
        });

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("Exploring files...", ctx => {
                explorer.Explore(rootPath, (current, total) => {
                    ctx.Status($"Exploring files ([yellow]{current}[/]/[yellow]{total}[/])...");
                });
                ctx.Status("Building code graph...");
                pipeline.BuildAndStoreGraph(outputRoot);
            });

        if (explorer.Errors.Any())
        {
            _view.ShowAnalysisFailed(explorer, config);
            return 1;
        }

        _view.ShowExplorationComplete(outputRoot);
        LogExplorationComplete(_logger, outputRoot);

        return 0;
    }
}
