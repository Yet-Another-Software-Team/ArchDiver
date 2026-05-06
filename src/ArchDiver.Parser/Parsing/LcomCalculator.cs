using ArchDiver.Parser.Abstractions;
using ArchDiver.Core.Models;
using Microsoft.Extensions.Logging;

namespace ArchDiver.Parser.Parsing;

public class LcomCalculator(ILanguageProvider provider, ILogger<LcomCalculator> logger)
{
    private readonly ILanguageProvider _provider = provider;
    private readonly ILogger<LcomCalculator> _logger = logger;

    /// <summary>
    /// Calculates the Lack of Cohesion in Methods (LCOM) for a component.
    /// LCOM = 1 - Average(MethodFieldUsageRatio)
    /// </summary>
    public double CalculateClassLcom(string componentName, List<HashSet<string>> methodFieldUsages, int totalFields)
    {
        if (totalFields == 0)
        {
            _logger.LogWarning("LCOM for '{ComponentName}' cannot be calculated: No fields found.", componentName);
            return 0;
        }

        if (methodFieldUsages.Count <= 1)
        {
            _logger.LogDebug("LCOM for '{ComponentName}' is 0: Single method or no methods found.", componentName);
            return 0;
        }

        double sumUsageRatio = 0;
        foreach (var usage in methodFieldUsages)
        {
            sumUsageRatio += (double)usage.Count / totalFields;
        }

        double avgUsageRatio = sumUsageRatio / methodFieldUsages.Count;
        return Math.Round(1.0 - avgUsageRatio, 4);
    }

    /// <summary>
    /// Calculates the per-method Lack of Cohesion.
    /// 1 - (FieldsUsedByMethod / TotalFieldsInClass)
    /// </summary>
    public double CalculateMethodLcom(string methodName, HashSet<string> usedFields, int totalFields)
    {
        if (totalFields == 0) return 0;
        return Math.Round(1.0 - ((double)usedFields.Count / totalFields), 4);
    }

    public HashSet<string> GetUsedFields(AstNode methodNode, List<string> fields)
    {
        var usedFields = new HashSet<string>();
        FindUsedIdentifiers(methodNode, fields, usedFields);
        return usedFields;
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
