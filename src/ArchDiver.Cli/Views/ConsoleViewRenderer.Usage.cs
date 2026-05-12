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
        Console.WriteLine("                               Pass --show-all to display all predictions, not just those exceeding the threshold.");
        Console.WriteLine("  config                       Shows current configuration.");
        Console.WriteLine("  config create                Creates a default configuration file.");
        Console.WriteLine($"\nSupported Languages: {string.Join(", ", supportedLanguages)}");
    }
}
