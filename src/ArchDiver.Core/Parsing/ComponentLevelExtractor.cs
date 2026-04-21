using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Models;

namespace ArchDiver.Core.Parsing;

public class ComponentLevelExtractor(ILanguageProvider provider, LcomCalculator lcomCalculator)
{
    private readonly ILanguageProvider _provider = provider;
    private readonly LcomCalculator _lcomCalculator = lcomCalculator;

    public ComponentResult Extract(AstNode classNode)
    {
        var component = new ComponentResult { Name = ExtractName(classNode) ?? "UnknownComponent" };
        var methods = new List<AstNode>();
        var fields = new List<string>();

        Traverse(classNode, methods, fields);
        component.Attribute = fields;

        foreach (var methodNode in methods)
        {
            component.Methods.Add(new MethodResult
            {
                Name = ExtractName(methodNode) ?? "unknown_method",
                Params = ExtractParams(methodNode),
                Lcom = _lcomCalculator.Calculate(methodNode, fields)
            });
        }
        return component;
    }

    private void Traverse(AstNode node, List<AstNode> methods, List<string> fields)
    {
        if (IsType(node, "Method")) { methods.Add(node); return; }
        if (IsType(node, "Field"))
        {
            var name = ExtractName(node);
            if (!string.IsNullOrEmpty(name)) fields.Add(name);
        }
        foreach (var child in node.Children) Traverse(child, methods, fields);
    }

    private string? ExtractName(AstNode node)
    {
        var nameNode = node.Children.FirstOrDefault(c => c.FieldName == "name");
        return nameNode?.Text ?? FindChildByType(node, "Identifier")?.Text;
    }

    private List<string> ExtractParams(AstNode node)
    {
        var paramsNode = node.Children.FirstOrDefault(c => c.FieldName == "parameters");
        if (paramsNode == null) return new List<string>();

        var paramNames = new List<string>();
        CollectParamNames(paramsNode, paramNames);
        return paramNames;
    }

    private void CollectParamNames(AstNode node, List<string> names)
    {
        if (node.FieldName == "name" || IsType(node, "Identifier"))
        {
            names.Add(node.Text);
            return;
        }
        foreach (var child in node.Children) CollectParamNames(child, names);
    }

    private bool IsType(AstNode node, string concept)
    {
        return _provider.NodeBindings.TryGetValue(concept, out var types) && types.Contains(node.Type);
    }

    private AstNode? FindChildByType(AstNode node, string concept)
    {
        return _provider.NodeBindings.TryGetValue(concept, out var types)
            ? node.Children.FirstOrDefault(c => types.Contains(c.Type))
            : null;
    }
}
