using ArchDiver.Core.Models;

namespace ArchDiver.Core.Abstractions;

public interface ICodeAnalysisEngine
{
    FileAnalysisResult Analyze(string sourceCode, string filePath);
    IEnumerable<string> GetSupportedLanguages();
}
