using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Models;
using ArchDiver.Parser.Abstractions;
using ArchDiver.Parser.Infrastructure;

namespace ArchDiver.Parser.Parsing;

public class ParserEngine(ILanguageRegistry registry) : ICodeAnalysisEngine
{
    private readonly ILanguageRegistry _registry = registry;

    public FileAnalysisResult Analyze(string sourceCode, string filePath)
    {
        var provider = _registry.Identify(filePath, sourceCode)
                       ?? throw new NotSupportedException($"No provider found for file: {filePath}");

        var parser = new CodeParser(provider);
        var ast = parser.Parse(sourceCode);

        var extractor = new ConceptExtractor(provider);
        return extractor.Extract(ast);
    }

    public IEnumerable<string> GetSupportedLanguages() => _registry.GetSupportedLanguages();
}
