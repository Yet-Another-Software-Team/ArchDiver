using ArchDiver.Parser.Abstractions;
using ArchDiver.Core.Models;

namespace ArchDiver.Parser.Parsing;

public class ComponentLevelExtractor(ILanguageProvider provider)
{
    private readonly ILanguageProvider _provider = provider;

    public ComponentResult Extract(AstNode classNode)
    {
        var result = new ComponentResult();
        var fields = new List<string>();

        ExtractMembers(classNode, result, fields);

        var lcomCalc = new LcomCalculator(_provider);
        foreach (var method in result.Methods)
        {
            // This is a simplified LCOM calculation placeholder
            // method.Lcom = lcomCalc.Calculate(methodNode, fields);
        }

        return result;
    }

    private void ExtractMembers(AstNode node, ComponentResult component, List<string> fields)
    {
        if (IsType(node, "ClassName"))
        {
            component.Name = node.Text;
        }
        else if (IsType(node, "FieldName"))
        {
            component.Attribute.Add(node.Text);
            fields.Add(node.Text);
        }
        else if (IsType(node, "Method"))
        {
            var method = new MethodResult { Name = GetMethodName(node) };
            component.Methods.Add(method);
        }

        foreach (var child in node.Children)
        {
            ExtractMembers(child, component, fields);
        }
    }

    private string GetMethodName(AstNode methodNode)
    {
        foreach (var child in methodNode.Children)
        {
            if (IsType(child, "MethodName")) return child.Text;
        }
        return "Unknown";
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
