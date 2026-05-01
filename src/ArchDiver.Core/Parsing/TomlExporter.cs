using System.Text.Json;
using Tomlyn;
using ArchDiver.Core.Models;
using ArchDiver.Core.Abstractions;

namespace ArchDiver.Core.Parsing;

public class TomlExporter : IExporter
{
    public void Export(FileAnalysisResult result, string outputDir)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        if (string.IsNullOrEmpty(outputDir)) throw new ArgumentException("Output directory cannot be empty.", nameof(outputDir));

        foreach (var comp in result.Components)
        {
            string componentPath = Path.Combine(outputDir, $"{comp.Name}.toml");
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
            var mTable = new Tomlyn.Model.TomlTable();
            mTable["name"] = m.Name;

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
