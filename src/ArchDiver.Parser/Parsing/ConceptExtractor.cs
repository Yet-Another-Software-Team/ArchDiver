using ArchDiver.Parser.Abstractions;
using ArchDiver.Core.Models;

namespace ArchDiver.Parser.Parsing;

public class ConceptExtractor(ILanguageProvider provider)
{
    private readonly ILanguageProvider _provider = provider;

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
        // Check for Class, Interface, Struct, etc. (mapped to "Class" in NodeBindings)
        if (IsType(node, "Class"))
        {
            var compExtractor = new ComponentLevelExtractor(_provider);
            components.Add(compExtractor.Extract(node));

            // Note: We don't recurse into the class node for other classes
            // unless we want to support nested classes, but they would be
            // separate components in our model for now.
            // For now, let's allow recursion to find nested types if they exist.
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
