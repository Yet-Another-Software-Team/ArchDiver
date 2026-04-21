using ArchDiver.Core.Infrastructure;
using ArchDiver.Core.Pipeline;

namespace ArchDiver.Cli;

class Program
{
    private static int _maxDepth = 10;
    private static string _outputDir = ".archdiver/out";

    static void Main(string[] args)
    {
        Bootstrapper.Initialize();
        if (args.Length < 1) { PrintUsage(); return; }

        string command = args[0].ToLower();
        if (command == "explore") HandleExplore(args);
        else PrintUsage();
    }

    static void PrintUsage()
    {
        Console.WriteLine("ArchDiver CLI\nUsage:\n  archdiver explore <directory_path> [--max-depth <n>]\n");
        Console.WriteLine("Commands:\n  explore  Recursively parses all supported files in a directory.\n");
        Console.WriteLine($"Supported Languages: {string.Join(", ", LanguageRegistry.GetSupportedLanguages())}");
    }

    static void HandleExplore(string[] args)
    {
        if (args.Length < 2) { Console.WriteLine("Error: Missing directory path."); return; }

        string rootPath = Path.GetFullPath(args[1]);
        if (!Directory.Exists(rootPath)) { Console.WriteLine($"Error: Directory not found: {rootPath}"); return; }

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--max-depth" && i + 1 < args.Length && int.TryParse(args[i + 1], out int depth))
                _maxDepth = depth;
        }

        string outputRoot = Path.Combine(rootPath, _outputDir);
        if (Directory.Exists(outputRoot)) Directory.Delete(outputRoot, true);
        Directory.CreateDirectory(outputRoot);

        Console.WriteLine($"Exploring directory: {rootPath} (Max Depth: {_maxDepth})");
        var explorer = new DirectoryExplorer(new PipelineControlUnit(), outputRoot, _maxDepth);
        explorer.Explore(rootPath, rootPath, 0);
        Console.WriteLine($"Exploration complete. Results saved in {outputRoot}");
    }
}
