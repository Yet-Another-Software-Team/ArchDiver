using ArchDiver.Parser.Abstractions;
using ArchDiver.Core.Models;

namespace ArchDiver.Parser.Parsing;

public class LcomCalculator(ILanguageProvider provider)
{
    private readonly ILanguageProvider _provider = provider;

    public double Calculate(AstNode methodNode, List<string> fields)
    {
        if (fields == null || fields.Count == 0) return 0;

        var usedFields = new HashSet<string>();
        FindUsedIdentifiers(methodNode, fields, usedFields);

        return (double)usedFields.Count / fields.Count;
    }

    private void FindUsedIdentifiers(AstNode node, List<string> fields, HashSet<string> usedFields)
    {
        if (IsType(node, "Identifier") && fields.Contains(node.Text))
        {
            usedFields.Add(node.Text);
        }

        foreach (var child in node.Children)
        {
            FindUsedIdentifiers(child, fields, usedFields);
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
