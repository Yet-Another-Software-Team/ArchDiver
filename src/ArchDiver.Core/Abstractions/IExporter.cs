using ArchDiver.Core.Models;

namespace ArchDiver.Core.Abstractions;

public interface IExporter
{
    void Export(FileAnalysisResult result, string outputDir);
}
