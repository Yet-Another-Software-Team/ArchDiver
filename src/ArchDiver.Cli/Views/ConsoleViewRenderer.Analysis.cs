using ArchDiver.Core.Models;
using ArchDiver.GraphConstruction;
using ArchDiver.Shared.Models;
using Spectre.Console;

namespace ArchDiver.Cli.Views;

public partial class ConsoleViewRenderer
{
    public void ShowExplorationComplete(string outputRoot)
    {
        AnsiConsole.MarkupLine("[green]Exploration complete.[/] Results saved in [blue]{0}[/]", outputRoot);
    }

    public void ShowAnalysisHeader(ProjectConfig config)
    {
        AnsiConsole.Write(new Rule("[yellow]Smell Analysis Predictions[/]"));
        AnsiConsole.MarkupLine($"[grey]Threshold: >= {config.Analysis.ConfidenceThreshold:F2}[/]");
    }

    public void ShowAnalysisResults(Graph graph, IDictionary<int, float> predictions, ProjectConfig config, bool showAll)
    {
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

    public void ShowAnalysisSummary(int scannedFiles, int smellsDetected)
    {
        AnsiConsole.MarkupLine("([yellow]{0}[/] file(s) scanned, [yellow]{1}[/] smell(s) detected)",
            scannedFiles, smellsDetected);
    }

    private static string GetHierarchicalName(Graph graph, Node node)
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
                break;
            }

            path.Insert(0, parentName);
            currentId = parent.Id;
        }

        return string.Join(".", path);
    }
}
