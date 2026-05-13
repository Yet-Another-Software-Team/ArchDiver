using ArchDiver.Core.Pipeline;
using ArchDiver.Core.Models;
using ArchDiver.GraphConstruction;
using ArchDiver.Shared.Models;

namespace ArchDiver.Cli.Views;

public interface IViewRenderer
{
    void ShowUsage(IEnumerable<string> supportedLanguages);
    void ShowConfig(ProjectConfig config, string configFileName);
    void ShowExplorationComplete(string outputRoot);
    void ShowAnalysisHeader(ProjectConfig config);
    void ShowAnalysisResults(Graph graph, IDictionary<int, float> predictions, ProjectConfig config, bool showAll);
    void ShowAnalysisSummary(int scannedFiles, int smellsDetected);
    void ShowAnalysisFailed(DirectoryExplorer explorer, ProjectConfig config);
}
