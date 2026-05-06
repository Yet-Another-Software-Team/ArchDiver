using ArchDiver.Parser.Abstractions;
using ArchDiver.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArchDiver.Parser.Parsing;

public class ConceptExtractor(ILanguageProvider provider, ILoggerFactory loggerFactory)
{
    private readonly ILanguageProvider _provider = provider;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    public FileAnalysisResult Extract(AstNode root)
    {
        var result = new FileAnalysisResult();
        var components = new List<ComponentResult>();

        ExtractRec(root, components, result.Imports);
        result.Components = components;

        return result;
    }

    private void ExtractRec(AstNode node, List<ComponentResult> components, List<string> imports)
    {
        if (IsType(node, "Class"))
        {
            var compExtractor = new ComponentLevelExtractor(_provider, _loggerFactory);
            components.Add(compExtractor.Extract(node));
        }
        else if (IsType(node, "Import"))
        {
            imports.Add(node.Text);
        }

        foreach (var child in node.Children)
        {
            ExtractRec(child, components, imports);
        }
    }

    private bool IsType(AstNode node, string concept)
    {
        if (_provider.NodeBindings.TryGetValue(concept, out var types))
        {
            return types.Contains(node.Type);
        }
        return false;
    }
}
