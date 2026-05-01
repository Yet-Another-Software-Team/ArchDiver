using ArchDiver.Parser.Abstractions;
using ArchDiver.Core.Models;

namespace ArchDiver.Parser.Parsing;

public class ConceptExtractor(ILanguageProvider provider)
{
    private readonly ILanguageProvider _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    private readonly ComponentLevelExtractor _componentExtractor = new(provider, new LcomCalculator(provider));

    public FileAnalysisResult Extract(AstNode root)
    {
        var result = new FileAnalysisResult();
        Traverse(root, result);
        return result;
    }

    private void Traverse(AstNode node, FileAnalysisResult result)
    {
        if (IsType(node, "Class"))
        {
            result.Components.Add(_componentExtractor.Extract(node));
            return;
        }
        if (IsType(node, "Import"))
        {
            var name = ExtractImportName(node);
            if (!string.IsNullOrEmpty(name)) result.Imports.Add(name);
        }
        foreach (var child in node.Children) Traverse(child, result);
    }

    private string? ExtractImportName(AstNode node)
    {
        var fields = new[] { "name", "module", "namespace", "path" };
        foreach (var field in fields)
        {
            var child = node.Children.FirstOrDefault(c => c.FieldName == field);
            if (child != null) return child.Text;
        }
        return FindFirstIdentifier(node);
    }

    private string? FindFirstIdentifier(AstNode node)
    {
        if (IsType(node, "Identifier"))
        {
            string text = node.Text.Trim();
            string[] kw = { "using", "import", "from", "package", "namespace", "static" };
            if (!kw.Contains(text)) return text;
        }
        foreach (var child in node.Children)
        {
            var res = FindFirstIdentifier(child);
            if (res != null) return res;
        }
        return null;
    }

    private bool IsType(AstNode node, string concept)
    {
        return _provider.NodeBindings.TryGetValue(concept, out var types) && types.Contains(node.Type);
    }
}
