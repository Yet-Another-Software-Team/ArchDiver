using ArchDiver.Shared.Models;
using Spectre.Console;

namespace ArchDiver.Cli.Views;

public partial class ConsoleViewRenderer
{
    public void ShowUsage(IEnumerable<string> supportedLanguages)
    {
        AnsiConsole.Write(new FigletText("ArchDiver").Color(Color.Blue));
        Console.WriteLine("\nUsage:\n  archdiver <command> [options]\n");
        Console.WriteLine("Commands:");
        Console.WriteLine("  explore <path>               Recursively parses supported files.");
        Console.WriteLine("  analyze <path> [model_path]  Runs full pipeline and prints smell predictions. If no model path is specified, uses the built-in model.");
        Console.WriteLine("                               Options:");
        Console.WriteLine("                                 --show-all    Display all predictions, not just those exceeding the threshold.");
        Console.WriteLine("                                 --ci          Summarize findings and exit with non-zero code if smells detected.");
        Console.WriteLine("                                 --from-toml   Run analysis from pre-parsed TOML artifacts instead of source code.");
        Console.WriteLine("                                 --max-depth   Override maximum directory search depth.");
        Console.WriteLine("  config [path]                Shows configuration from nearest archdiver.toml or specified path.");
        Console.WriteLine("  config create [path]         Creates a default configuration file.");
        Console.WriteLine($"\nSupported Languages: {string.Join(", ", supportedLanguages)}");
    }
}
