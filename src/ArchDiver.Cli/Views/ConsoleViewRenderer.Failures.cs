using ArchDiver.Core.Pipeline;
using ArchDiver.Core.Models;
using Spectre.Console;

namespace ArchDiver.Cli.Views;

public partial class ConsoleViewRenderer
{
    public void ShowAnalysisFailed(DirectoryExplorer explorer, ProjectConfig config)
    {
        AnsiConsole.Write(new Rule("[red]Analysis Failed[/]"));
        AnsiConsole.MarkupLine("[red]One or more errors occurred during analysis. Results are incomplete and will not be shown.[/]");

        var errorTable = new Table().Border(TableBorder.Rounded);
        errorTable.AddColumn("Error Category");
        errorTable.AddColumn("Affected Files");

        var groupedErrors = explorer.Errors.GroupBy(e => e.Exception.Message);
        foreach (var group in groupedErrors)
        {
            string escapedMessage = Markup.Escape(group.Key);
            errorTable.AddRow(
                $"[red]{escapedMessage}[/]",
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
}
