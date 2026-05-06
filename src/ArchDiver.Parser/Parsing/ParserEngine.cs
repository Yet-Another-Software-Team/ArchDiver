using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Models;
using ArchDiver.Parser.Abstractions;
using ArchDiver.Parser.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ArchDiver.Parser.Parsing;

public class ParserEngine(ILanguageRegistry registry, ILoggerFactory loggerFactory) : ICodeAnalysisEngine
{
    private readonly ILanguageRegistry _registry = registry;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    public FileAnalysisResult Analyze(string sourceCode, string filePath)
    {
        var provider = _registry.Identify(filePath, sourceCode)
                       ?? throw new NotSupportedException($"No provider found for file: {filePath}");

        var parser = new CodeParser(provider);
        var ast = parser.Parse(sourceCode);

        var extractor = new ConceptExtractor(provider, _loggerFactory);
        return extractor.Extract(ast);
    }

    public IEnumerable<string> GetSupportedLanguages() => _registry.GetSupportedLanguages();
}
