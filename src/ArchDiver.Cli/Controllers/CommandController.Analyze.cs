using ArchDiver.Core.Models;
using ArchDiver.Core.Infrastructure;
using ArchDiver.Core.Pipeline;
using ArchDiver.Shared.Models;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ArchDiver.Cli.Controllers;

public partial class CommandController
{
    private int HandleAnalyze(string[] args, ProjectConfig config)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Missing directory path. Usage: archdiver analyze <path> [model_path] [--show-all] [--ci]");
            return 1;
        }

        string rootPath = Path.GetFullPath(args[1]);
        string? modelPath = null;
        bool showAll = false;
        bool ciMode = false;
        int maxDepth = config.Analysis.MaxDepth;

        for (int i = 2; i < args.Length; i++)
        {
            if (args[i] == "--show-all")
            {
                showAll = true;
            }
            else if (args[i] == "--ci")
            {
                ciMode = true;
            }
            else if (args[i] == "--max-depth" && i + 1 < args.Length && int.TryParse(args[i + 1], out int depth))
            {
                maxDepth = depth;
                i++;
            }
            else if (!args[i].StartsWith("--") && modelPath == null)
            {
                modelPath = Path.GetFullPath(args[i]);
            }
        }

        if (!Directory.Exists(rootPath))
        {
            LogDirectoryNotFound(_logger, rootPath);
            return 1;
        }

        if (modelPath != null && !File.Exists(modelPath))
        {
            LogModelNotFound(_logger, modelPath);
            return 1;
        }

        string outputRoot = Path.Combine(rootPath, _outputDir);
        if (Directory.Exists(outputRoot)) Directory.Delete(outputRoot, true);
        Directory.CreateDirectory(outputRoot);

        if (modelPath != null)
        {
            LogAnalyzingWithModel(_logger, rootPath, maxDepth, modelPath);
        }
        else
        {
            LogAnalyzingWithBuiltInModel(_logger, rootPath, maxDepth);
        }

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

        Graph graph = null!;
        IDictionary<int, float> predictions = null!;
        int scannedFiles = 0;

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("Analyzing project...", ctx => {
                explorer.Explore(rootPath, (current, total) => {
                    ctx.Status($"Exploring files ([yellow]{current}[/]/[yellow]{total}[/])...");
                    scannedFiles = total;
                });

                ctx.Status("Building code graph...");
                graph = pipeline.BuildAndStoreGraph(outputRoot);

                ctx.Status("Running smell analysis...");
                predictions = pipeline.AnalyzeSmells(modelPath);
            });

        if (explorer.Errors.Any())
        {
            _view.ShowAnalysisFailed(explorer, config);
            return 1;
        }

        _view.ShowAnalysisHeader(config);
        _view.ShowAnalysisResults(graph, predictions, config, showAll);

        int smellsDetected = predictions.Values.Count(v => v >= config.Analysis.ConfidenceThreshold);

        if (ciMode)
        {
            _view.ShowAnalysisSummary(scannedFiles, smellsDetected);
        }

        return smellsDetected > 0 ? 1 : 0;
    }
}
