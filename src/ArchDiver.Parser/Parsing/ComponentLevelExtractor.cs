using ArchDiver.Parser.Abstractions;
using ArchDiver.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArchDiver.Parser.Parsing;

public class ComponentLevelExtractor(ILanguageProvider provider, ILoggerFactory loggerFactory)
{
    private readonly ILanguageProvider _provider = provider;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    public ComponentResult Extract(AstNode classNode)
    {
        var result = new ComponentResult();
        var fields = new HashSet<string>();
        var methodsWithNodes = new List<(MethodResult Result, AstNode Node)>();

        ExtractMembers(classNode, result, fields, methodsWithNodes);

        var lcomCalc = new LcomCalculator(_provider, _loggerFactory.CreateLogger<LcomCalculator>());
        var methodFieldUsages = new List<HashSet<string>>();
        var fieldList = fields.ToList();
        result.Attribute = fieldList;

        foreach (var (methodResult, methodNode) in methodsWithNodes)
        {
            var usedFields = lcomCalc.GetUsedFields(methodNode, fieldList);
            methodFieldUsages.Add(usedFields);
            methodResult.Lcom = lcomCalc.CalculateMethodLcom(methodResult.Name, usedFields, fieldList.Count);
        }

        result.Lcom = lcomCalc.CalculateClassLcom(result.Name, methodFieldUsages, fieldList.Count);

        return result;
    }

    private void ExtractMembers(AstNode node, ComponentResult component, HashSet<string> fields, List<(MethodResult Result, AstNode Node)> methodsWithNodes)
    {
        if (string.IsNullOrEmpty(component.Name) && IsType(node, "Class"))
        {
            component.Name = GetNamedChildText(node, "name") ?? "UnnamedComponent";
        }

        if (IsType(node, "Field"))
        {
            string? name = GetNamedChildText(node, "name");
            if (name != null)
            {
                fields.Add(name);
            }
            if (name != null && node.Type != "field_declaration") return;
        }
        else if (IsType(node, "Method"))
        {
            var method = new MethodResult
            {
                Name = GetNamedChildText(node, "name") ?? "UnknownMethod"
            };
            component.Methods.Add(method);
            methodsWithNodes.Add((method, node));
            return;
        }

        foreach (var child in node.Children)
        {
            ExtractMembers(child, component, fields, methodsWithNodes);
        }
    }

    private string? GetNamedChildText(AstNode node, string fieldName)
    {
        foreach (var child in node.Children)
        {
            if (child.FieldName == fieldName) return child.Text;
        }

        foreach (var child in node.Children)
        {
            if (child.Type == "variable_declaration" || child.Type == "variable_declarator")
            {
                var nested = GetNamedChildText(child, fieldName);
                if (nested != null) return nested;
            }
        }

        return null;
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
