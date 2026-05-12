using ArchDiver.Parser.Abstractions;
using ArchDiver.Core.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

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

        bool isMethod = IsType(node, "Method");
        if (isMethod)
        {
            var method = new MethodResult
            {
                Name = GetNamedChildText(node, "name") ?? "UnknownMethod"
            };
            component.Methods.Add(method);
            methodsWithNodes.Add((method, node));
        }

        if (_provider.LanguageId == "Python")
        {
            ExtractPythonFields(node, fields);
        }
        else if (IsType(node, "Field"))
        {
            string? name = GetNamedChildText(node, "name");
            if (name != null)
            {
                fields.Add(name);
            }
        }

        foreach (var child in node.Children)
        {
            if (isMethod && _provider.LanguageId != "Python") continue;
            ExtractMembers(child, component, fields, methodsWithNodes);
        }
    }

    private void ExtractPythonFields(AstNode node, HashSet<string> fields)
    {
        if (node.Type == "attribute")
        {
            var obj = node.Children.FirstOrDefault(c => c.FieldName == "object");
            if (obj?.Text == "self")
            {
                var attr = node.Children.FirstOrDefault(c => c.FieldName == "attribute");
                if (attr != null) fields.Add(attr.Text);
            }
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
            // C#: variable_declarator has 'name'
            // Java: variable_declarator has 'name'
            if (child.Type.Contains("declaration") || child.Type.Contains("declarator") || child.Type.Contains("designation") || child.Type == "variable_declaration")
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
