using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using ArchDiver.Core.Pipeline;
using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Infrastructure;
using ArchDiver.Core.Models;
using ArchDiver.Parser.Infrastructure;
using ArchDiver.Shared.Models;
using Spectre.Console;
using Serilog;

namespace ArchDiver.Cli;

class Program
{
    private static readonly string _configFileName = "archdiver.toml";
    private static readonly string _outputDir = ".archdiver/out";
    private static ILoggerFactory _loggerFactory = null!;
    private static ILogger<Program> _logger = null!;
    private static ICodeAnalysisEngine _analysisEngine = null!;
    private static readonly IConfigManager _configManager = new TomlConfigManager();

    static void Main(string[] args)
    {
        ProjectConfig config = File.Exists(_configFileName)
            ? _configManager.Load(_configFileName)
            : _configManager.GetDefault();

        ConfigureLogging(config);
        _analysisEngine = Bootstrapper.Initialize(_loggerFactory);

        if (args.Length < 1) { PrintUsage(); return; }
        string command = args[0].ToLower();
        switch (command)
        {
            case "explore": HandleExplore(args, config); break;
            case "analyze": HandleAnalyze(args, config); break;
            case "config": HandleConfig(args); break;
            default: PrintUsage(); break;
        }
    }

    static void ConfigureLogging(ProjectConfig config)
    {
        var serilogLoggerConfiguration = new Serilog.LoggerConfiguration()
            .MinimumLevel.Is((Serilog.Events.LogEventLevel)config.Logging.MinimumLevel);

        if (!string.IsNullOrWhiteSpace(config.Logging.LogFilePath))
        {
            serilogLoggerConfiguration.WriteTo.File(config.Logging.LogFilePath);
        }

        Serilog.Log.Logger = serilogLoggerConfiguration.CreateLogger();

        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(Serilog.Log.Logger);
        });
        _logger = _loggerFactory.CreateLogger<Program>();
    }

    static void PrintUsage()
    {
        AnsiConsole.Write(new FigletText("ArchDiver").Color(Color.Blue));
        Console.WriteLine("\nUsage:\n  archdiver <command> [options]\n");
        Console.WriteLine("Commands:");
        Console.WriteLine("  explore <path>               Recursively parses supported files.");
        Console.WriteLine("  analyze <path> [model_path]  Runs full pipeline and prints smell predictions. If no model path is specified, uses the built-in model.");
        Console.WriteLine("                               Pass --show-all to display all predictions, not just those exceeding the threshold.");
        Console.WriteLine("  config                       Shows current configuration.");
        Console.WriteLine("  config create                Creates a default configuration file.");
        Console.WriteLine($"\nSupported Languages: {string.Join(", ", _analysisEngine.GetSupportedLanguages())}");
    }

    static void HandleConfig(string[] args)
    {
        if (args.Length > 1 && args[1].ToLower() == "create")
        {
            _configManager.Save(_configManager.GetDefault(), _configFileName);
            _logger.LogInformation("Created default configuration: {ConfigFileName}", _configFileName);
            return;
        }

        if (!File.Exists(_configFileName))
        {
            _logger.LogWarning("No config file found. Using defaults.");
            DisplayConfig(_configManager.GetDefault());
        }
        else
        {
            DisplayConfig(_configManager.Load(_configFileName));
        }
    }

    static void DisplayConfig(ProjectConfig config)
    {
        var table = new Table();
        table.AddColumn("Setting");
        table.AddColumn("Value");
        table.AddRow("Log Level", config.Logging.MinimumLevel.ToString());
        table.AddRow("Max Depth", config.Analysis.MaxDepth.ToString());
        table.AddRow("Confidence Threshold", config.Analysis.ConfidenceThreshold.ToString("F2"));
        table.AddRow("Ignore Patterns", string.Join(", ", config.Analysis.IgnorePatterns));
        
        AnsiConsole.Write(new Rule($"[yellow]Configuration ({_configFileName})[/]"));
        AnsiConsole.Write(table);
    }

    static void HandleExplore(string[] args, ProjectConfig config)
    {
        if (args.Length < 2) { _logger.LogError("Missing directory path."); return; }

        string rootPath = Path.GetFullPath(args[1]);
        if (!Directory.Exists(rootPath)) { _logger.LogError("Directory not found: {RootPath}", rootPath); return; }

        int maxDepth = config.Analysis.MaxDepth;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--max-depth" && i + 1 < args.Length && int.TryParse(args[i + 1], out int depth))
                maxDepth = depth;
        }

        string outputRoot = Path.Combine(rootPath, _outputDir);
        if (Directory.Exists(outputRoot)) Directory.Delete(outputRoot, true);
        Directory.CreateDirectory(outputRoot);

        _logger.LogInformation("Exploring: {RootPath} (Max Depth: {MaxDepth})", rootPath, maxDepth);

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
            ReportFailures(explorer, rootPath, config);
            return;
        }

        AnsiConsole.MarkupLine("[green]Exploration complete.[/] Results saved in [blue]{0}[/]", outputRoot);
        _logger.LogInformation("Exploration complete. Results saved in {OutputRoot}", outputRoot);
    }

    static void HandleAnalyze(string[] args, ProjectConfig config)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Missing directory path. Usage: archdiver analyze <path> [model_path] [--show-all]");
            return;
        }

        string rootPath = Path.GetFullPath(args[1]);
        string? modelPath = null;
        bool showAll = false;
        int maxDepth = config.Analysis.MaxDepth;

        for (int i = 2; i < args.Length; i++)
        {
            if (args[i] == "--show-all")
            {
                showAll = true;
            }
            else if (args[i] == "--max-depth" && i + 1 < args.Length && int.TryParse(args[i + 1], out int depth))
            {
                maxDepth = depth;
                i++; // Skip the depth value in the next iteration
            }
            else if (!args[i].StartsWith("--") && modelPath == null)
            {
                modelPath = Path.GetFullPath(args[i]);
            }
        }

        if (!Directory.Exists(rootPath)) { _logger.LogError("Directory not found: {RootPath}", rootPath); return; }
        if (modelPath != null && !File.Exists(modelPath)) { _logger.LogError("Model not found: {ModelPath}", modelPath); return; }

        string outputRoot = Path.Combine(rootPath, _outputDir);
        if (Directory.Exists(outputRoot)) Directory.Delete(outputRoot, true);
        Directory.CreateDirectory(outputRoot);

        if (modelPath != null)
        {
            _logger.LogInformation("Analyzing: {RootPath} (Max Depth: {MaxDepth}) with model {ModelPath}", rootPath, maxDepth, modelPath);
        }
        else
        {
            _logger.LogInformation("Analyzing: {RootPath} (Max Depth: {MaxDepth}) with built-in model", rootPath, maxDepth);
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

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("Analyzing project...", ctx => {
                explorer.Explore(rootPath, (current, total) => {
                    ctx.Status($"Exploring files ([yellow]{current}[/]/[yellow]{total}[/])...");
                });
                
                ctx.Status("Building code graph...");
                graph = pipeline.BuildAndStoreGraph(outputRoot);

                ctx.Status("Running smell analysis...");
                predictions = pipeline.AnalyzeSmells(modelPath);
            });

        if (explorer.Errors.Any())
        {
            ReportFailures(explorer, rootPath, config);
            return;
        }

        AnsiConsole.Write(new Rule("[yellow]Smell Analysis Predictions[/]"));
        AnsiConsole.MarkupLine($"[grey]Threshold: >= {config.Analysis.ConfidenceThreshold:F2}[/]");

        var resultsTable = new Table();
        resultsTable.AddColumn("Node");
        resultsTable.AddColumn("Confidence");
        resultsTable.AddColumn("Result");

        bool anyDetected = false;
        foreach (var kvp in predictions)
        {
            var node = graph.Nodes.FirstOrDefault(n => n.Id == kvp.Key);
            string nodeName = node != null ? GetHierarchicalName(graph, node) : $"Node {kvp.Key}";

            if (kvp.Value >= config.Analysis.ConfidenceThreshold)
            {
                resultsTable.AddRow(nodeName, $"[red]{kvp.Value:F4}[/]", "[red bold]Feature Concentration Detected[/]");
                anyDetected = true;
            }
            else if (showAll)
            {
                resultsTable.AddRow(nodeName, kvp.Value.ToString("F4"), "[grey]Negative[/]");
            }
        }

        if (anyDetected || (showAll && predictions.Any()))
        {
            AnsiConsole.Write(resultsTable);
        }
        else
        {
            AnsiConsole.MarkupLine("[green]No Architecture smell detected.[/] (Pass --show-all to see all predictions)");
        }
        AnsiConsole.Write(new Rule());
    }

    static void ReportFailures(DirectoryExplorer explorer, string rootPath, ProjectConfig config)
    {
        AnsiConsole.Write(new Rule("[red]Analysis Failed[/]"));
        AnsiConsole.MarkupLine("[red]One or more errors occurred during analysis. Results are incomplete and will not be shown.[/]");
        
        var errorTable = new Table().Border(TableBorder.Rounded);
        errorTable.AddColumn("Error Category");
        errorTable.AddColumn("Affected Files");

        var groupedErrors = explorer.Errors.GroupBy(e => e.Exception.Message);
        foreach (var group in groupedErrors)
        {
            errorTable.AddRow(
                $"[red]{group.Key}[/]",
                $"[grey]{group.Count()} file(s)[/]"
            );
        }
        AnsiConsole.Write(errorTable);

        if (explorer.Errors.Any(e => e.Exception.Message.Contains("Tree-sitter")))
        {
            AnsiConsole.Write(new Panel(new Rows(
                new Markup("[yellow bold]Missing Tree-sitter Native Libraries[/]"),
                new Markup("\nTo fix this, you need to provide the language-specific shared libraries:"),
                new Markup("1. Download the required [blue].dll[/] (Windows), [blue].so[/] (Linux), or [blue].dylib[/] (macOS) for the language."),
                new Markup("2. Place them in the application directory or ensure they are in your [blue]PATH[/]."),
                new Markup("3. Libraries should be named like [blue]tree-sitter-c-sharp[/], [blue]tree-sitter-java[/], etc.")
            )).BorderColor(Color.Yellow).Header("[yellow]Action Required[/]"));
        }

        if (!string.IsNullOrEmpty(config.Logging.LogFilePath))
        {
            AnsiConsole.MarkupLine($"[yellow]Check the log file for full details:[/] [blue]{config.Logging.LogFilePath}[/]");
        }
        AnsiConsole.Write(new Rule());
    }

    static string GetHierarchicalName(Graph graph, Node node)
    {
        var path = new List<string> { node.Name };
        int currentId = node.Id;

        while (true)
        {
            var edge = graph.Edges.FirstOrDefault(e => e.TargetId == currentId &&
                (e.Type == EdgeType.ComponentContainsClass || e.Type == EdgeType.ComponentContainsComponent));

            if (edge == null) break;

            var parent = graph.Nodes.FirstOrDefault(n => n.Id == edge.SourceId);
            if (parent == null) break;

            string parentName = parent.Name;
            if (parentName == "out")
            {
                break; // Root reached, skip adding it to the path
            }

            path.Insert(0, parentName);
            currentId = parent.Id;
        }

        return string.Join(".", path);
    }
}
