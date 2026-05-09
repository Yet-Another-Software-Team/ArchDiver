using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<string, CodeParser> _parsers = new();
    private readonly ConcurrentDictionary<string, ConceptExtractor> _extractors = new();

    public FileAnalysisResult Analyze(string sourceCode, string filePath)
    {
        var provider = _registry.Identify(filePath, sourceCode)
                       ?? throw new NotSupportedException($"No provider found for file: {filePath}");

        var parser = _parsers.GetOrAdd(provider.LanguageId, _ => new CodeParser(provider));
        var extractor = _extractors.GetOrAdd(provider.LanguageId, _ => new ConceptExtractor(provider, _loggerFactory));

        var ast = parser.Parse(sourceCode);
        return extractor.Extract(ast);
    }

    public IEnumerable<string> GetSupportedLanguages() => _registry.GetSupportedLanguages();
}
