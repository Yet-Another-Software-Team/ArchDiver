using ArchDiver.Core.Pipeline;
using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Infrastructure;
using ArchDiver.Core.Models;
using ArchDiver.Parser.Infrastructure;

namespace ArchDiver.Cli;

class Program
{
    private static readonly string _configFileName = "archdiver.toml";
    private static readonly string _outputDir = ".archdiver/out";
    private static IArchLogger _logger = new ConsoleLogger();
    private static ICodeAnalysisEngine _analysisEngine = null!;
    private static readonly IConfigManager _configManager = new TomlConfigManager();

    static void Main(string[] args)
    {
        _analysisEngine = Bootstrapper.Initialize();
        if (args.Length < 1) { PrintUsage(); return; }

        string command = args[0].ToLower();
        switch (command)
        {
            case "explore": HandleExplore(args); break;
            case "config": HandleConfig(args); break;
            default: PrintUsage(); break;
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("ArchDiver CLI\nUsage:\n  archdiver <command> [options]\n");
        Console.WriteLine("Commands:");
        Console.WriteLine("  explore <path>   Recursively parses supported files.");
        Console.WriteLine("  config           Shows current configuration.");
        Console.WriteLine("  config create    Creates a default configuration file.");
        Console.WriteLine($"\nSupported Languages: {string.Join(", ", _analysisEngine.GetSupportedLanguages())}");
    }

    static void HandleConfig(string[] args)
    {
        if (args.Length > 1 && args[1].ToLower() == "create")
        {
            _configManager.Save(_configManager.GetDefault(), _configFileName);
            _logger.LogInfo($"Created default configuration: {_configFileName}");
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
        Console.WriteLine($"Configuration ({_configFileName}):");
        Console.WriteLine($"  Log Level: {config.Logging.Level}");
        Console.WriteLine($"  Max Depth: {config.Analysis.MaxDepth}");
        Console.WriteLine($"  Ignore Patterns: {string.Join(", ", config.Analysis.IgnorePatterns)}");
    }

    static void HandleExplore(string[] args)
    {
        if (args.Length < 2) { _logger.LogError("Missing directory path."); return; }

        string rootPath = Path.GetFullPath(args[1]);
        if (!Directory.Exists(rootPath)) { _logger.LogError($"Directory not found: {rootPath}"); return; }

        ProjectConfig config = File.Exists(_configFileName)
            ? _configManager.Load(_configFileName)
            : _configManager.GetDefault();

        // Update max depth if passed via CLI (overrides config)
        int maxDepth = config.Analysis.MaxDepth;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--max-depth" && i + 1 < args.Length && int.TryParse(args[i + 1], out int depth))
                maxDepth = depth;
        }

        string outputRoot = Path.Combine(rootPath, _outputDir);
        if (Directory.Exists(outputRoot)) Directory.Delete(outputRoot, true);
        Directory.CreateDirectory(outputRoot);

        _logger.LogInfo($"Exploring: {rootPath} (Max Depth: {maxDepth})");

        var pipeline = new PipelineControlUnit(_analysisEngine, logger: _logger);
        var exporter = new TomlExporter();

        var explorer = new DirectoryExplorer(pipeline, _logger, maxDepth, config.Analysis.IgnorePatterns, (filePath, result) =>
        {
            string relativePath = Path.GetRelativePath(rootPath, filePath);
            string? directoryName = Path.GetDirectoryName(relativePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(relativePath);
            string fileOutputDir = Path.Combine(outputRoot, directoryName ?? "", fileNameWithoutExtension);

            exporter.Export(result, fileOutputDir);
        });
        explorer.Explore(rootPath);
        _logger.LogInfo($"Exploration complete. Results saved in {outputRoot}");
    }
}
