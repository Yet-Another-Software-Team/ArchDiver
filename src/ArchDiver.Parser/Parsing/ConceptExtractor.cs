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

        // Handle languages like Python where the file itself can act as a module/component
        if (components.Count == 0 && HasStrayMethods(root))
        {
            var compExtractor = new ComponentLevelExtractor(_provider, _loggerFactory);
            var moduleComponent = compExtractor.Extract(root);
            if (string.IsNullOrEmpty(moduleComponent.Name) || moduleComponent.Name == "UnnamedComponent")
            {
                moduleComponent.Name = "GlobalModule";
            }
            components.Add(moduleComponent);
        }

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
            ExtractImportNames(node, imports);
        }

        foreach (var child in node.Children)
        {
            ExtractRec(child, components, imports);
        }
    }

    private bool HasStrayMethods(AstNode node)
    {
        if (IsType(node, "Method"))
            return true;

        // Stop checking if we hit a Class, since methods inside a Class are not stray
        if (IsType(node, "Class"))
            return false;

        foreach (var child in node.Children)
        {
            if (HasStrayMethods(child))
                return true;
        }

        return false;
    }

    private void ExtractImportNames(AstNode node, List<string> imports)
    {
        // For Python import_from_statement, the first dotted_name is the module
        if (node.Type == "import_from_statement")
        {
            var moduleNode = node.Children.FirstOrDefault(c => IsType(c, "ImportName"));
            if (moduleNode != null)
            {
                imports.Add(moduleNode.Text);
                return;
            }
        }

        // For other imports, we collect all dotted names or identifiers as distinct imports
        bool found = false;
        foreach (var child in node.Children)
        {
            if (IsType(child, "ImportName"))
            {
                imports.Add(child.Text);
                found = true;
            }
        }

        // Fallback if no specific ImportName found
        if (!found)
        {
            imports.Add(node.Text);
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
