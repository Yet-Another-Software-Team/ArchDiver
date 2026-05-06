using ArchDiver.Core.Pipeline;
using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Infrastructure;
using ArchDiver.Parser.Infrastructure;

namespace ArchDiver.Cli;

class Program
{
    private static int _maxDepth = 10;
    private static readonly string _outputDir = ".archdiver/out";
    private static readonly IArchLogger _logger = new ConsoleLogger();
    private static ICodeAnalysisEngine _analysisEngine = null!;

    static void Main(string[] args)
    {
        _analysisEngine = Bootstrapper.Initialize();
        if (args.Length < 1) { PrintUsage(); return; }

        string command = args[0].ToLower();
        if (command == "explore") HandleExplore(args);
        else PrintUsage();
    }

    static void PrintUsage()
    {
        Console.WriteLine("ArchDiver CLI\nUsage:\n  archdiver explore <directory_path> [--max-depth <n>]\n");
        Console.WriteLine("Commands:\n  explore  Recursively parses all supported files in a directory.\n");
        Console.WriteLine($"Supported Languages: {string.Join(", ", _analysisEngine.GetSupportedLanguages())}");
    }

    static void HandleExplore(string[] args)
    {
        if (args.Length < 2) { _logger.LogError("Missing directory path."); return; }

        string rootPath = Path.GetFullPath(args[1]);
        if (!Directory.Exists(rootPath)) { _logger.LogError($"Directory not found: {rootPath}"); return; }

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--max-depth" && i + 1 < args.Length && int.TryParse(args[i + 1], out int depth))
                _maxDepth = depth;
        }

        string outputRoot = Path.Combine(rootPath, _outputDir);
        if (Directory.Exists(outputRoot)) Directory.Delete(outputRoot, true);
        Directory.CreateDirectory(outputRoot);

        _logger.LogInfo($"Exploring directory: {rootPath} (Max Depth: {_maxDepth})");

        var pipeline = new PipelineControlUnit(_analysisEngine, logger: _logger);
        var exporter = new TomlExporter();

        var explorer = new DirectoryExplorer(pipeline, _logger, _maxDepth, (filePath, result) =>
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
