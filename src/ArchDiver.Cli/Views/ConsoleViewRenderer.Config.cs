using ArchDiver.Core.Models;
using Spectre.Console;

namespace ArchDiver.Cli.Views;

public partial class ConsoleViewRenderer
{
    public void ShowConfig(ProjectConfig config, string configFileName)
    {
        var table = new Table();
        table.AddColumn("Setting");
        table.AddColumn("Value");
        table.AddRow("Log Level", config.Logging.MinimumLevel.ToString());
        table.AddRow("Max Depth", config.Analysis.MaxDepth.ToString());
        table.AddRow("Confidence Threshold", config.Analysis.ConfidenceThreshold.ToString("F2"));
        table.AddRow("Ignore Patterns", string.Join(", ", config.Analysis.IgnorePatterns));

        AnsiConsole.Write(new Rule($"[yellow]Configuration ({configFileName})[/]"));
        AnsiConsole.Write(table);
    }
}
