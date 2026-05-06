using System.Text.Json;
using Tomlyn;
using ArchDiver.Core.Models;
using ArchDiver.Core.Abstractions;

namespace ArchDiver.Core.Infrastructure;

public class TomlExporter : IExporter
{
    public void Export(FileAnalysisResult result, string outputDir)
    {
        Export(result, outputDir, null);
    }

    /// <summary>
    /// Exports the analysis results to TOML files.
    /// </summary>
    /// <param name="result">The analysis result.</param>
    /// <param name="outputDir">The directory to save files in.</param>
    /// <param name="namePrefix">Optional prefix for the filenames (e.g. the source filename).</param>
    public void Export(FileAnalysisResult result, string outputDir, string? namePrefix = null)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        if (string.IsNullOrEmpty(outputDir)) throw new ArgumentException("Output directory cannot be empty.", nameof(outputDir));

        foreach (var comp in result.Components)
        {
            string baseName = string.IsNullOrWhiteSpace(comp.Name) ? "unnamed_component" : comp.Name;

            // If we have a prefix and it's not the same as the component name, use [Prefix].[ComponentName]
            string fileName = (string.IsNullOrEmpty(namePrefix) || namePrefix == baseName)
                ? baseName
                : $"{namePrefix}.{baseName}";

            string componentPath = Path.Combine(outputDir, $"{fileName}.toml");
            ExportComponent(comp, result.Imports, componentPath);
        }
    }

    private void ExportComponent(ComponentResult component, List<string> imports, string outputPath)
    {
        if (component == null) throw new ArgumentNullException(nameof(component));
        if (string.IsNullOrEmpty(outputPath)) throw new ArgumentException("Output path cannot be empty.", nameof(outputPath));

        var options = new TomlSerializerOptions
        {
            WriteIndented = true,
            IndentSize = 4,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        var root = ToTomlModel(component, imports);
        string toml = TomlSerializer.Serialize(root, options);

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(outputPath, toml);
    }

    private static Tomlyn.Model.TomlTable ToTomlModel(ComponentResult comp, List<string> imports)
    {
        var root = new Tomlyn.Model.TomlTable
        {
            ["name"] = comp.Name
        };

        var attrArray = new Tomlyn.Model.TomlArray();
        foreach (var a in comp.Attribute) attrArray.Add(a);
        root["attribute"] = attrArray;

        root["num_methods"] = comp.NumMethods;

        var methods = new Tomlyn.Model.TomlTableArray();
        foreach (var m in comp.Methods)
        {
            var mTable = new Tomlyn.Model.TomlTable
            {
                ["name"] = m.Name
            };

            var pArray = new Tomlyn.Model.TomlArray();
            foreach (var p in m.Params) pArray.Add(p);
            mTable["params"] = pArray;

            mTable["lcom"] = Math.Round(m.Lcom, 2);
            methods.Add(mTable);
        }
        root["methods"] = methods;

        var importArray = new Tomlyn.Model.TomlArray();
        foreach (var i in imports) importArray.Add(i);
        root["imports"] = importArray;

        return root;
    }
}
