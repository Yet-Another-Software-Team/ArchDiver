using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tomlyn;
using Tomlyn.Model;
using ArchDiver.Shared.Models;

namespace ArchDiver.GraphConstruction;

public class GraphBuilder
{
    public Graph BuildCodeGraph(string rootDirectory)
    {
        var graph = new Graph();
        int nextNodeId = 0;
        var classNameToId = new Dictionary<string, int>();
        var componentPathToId = new Dictionary<string, int>();
        var classDependencies = new Dictionary<string, HashSet<string>>();

        void Walk(string currentDir)
        {
            var absDir = Path.GetFullPath(currentDir);

            if (!componentPathToId.TryGetValue(absDir, out int currCompId))
            {
                currCompId = nextNodeId++;
                componentPathToId[absDir] = currCompId;
            }

            string[] dirnames = Directory.GetDirectories(absDir);
            string[] filenames = Directory.GetFiles(absDir);
            var validFiles = filenames.Where(f => f.EndsWith(".toml", StringComparison.OrdinalIgnoreCase)).ToArray();

            graph.Nodes.Add(new Node
            {
                Id = currCompId,
                Name = Path.GetFileName(absDir),
                Type = NodeType.Component,
                Features = new double[] { dirnames.Length + validFiles.Length }
            });

            foreach (var dirname in dirnames)
            {
                var subDir = Path.GetFullPath(dirname);
                if (!componentPathToId.TryGetValue(subDir, out int subCompId))
                {
                    subCompId = nextNodeId++;

                    componentPathToId[subDir] = subCompId;
                }

                graph.Edges.Add(new Edge
                {
                    SourceId = currCompId,
                    TargetId = subCompId,
                    Type = EdgeType.ComponentContainsComponent
                });
            }

            foreach (var file in validFiles)
            {
                var tomlContent = File.ReadAllText(file);
                var tomlTable = TomlSerializer.Deserialize<TomlTable>(tomlContent);
                if (tomlTable == null) continue;

                string className = tomlTable.TryGetValue("name", out var nameObj)
                    ? nameObj?.ToString() ?? Path.GetFileNameWithoutExtension(file)
                    : Path.GetFileNameWithoutExtension(file);

                if (!classNameToId.TryGetValue(className, out int currClassId))
                {
                    currClassId = nextNodeId++;
                    classNameToId[className] = currClassId;
                }

                var features = ExtractClassFeatures(tomlTable);

                graph.Nodes.Add(new Node
                {
                    Id = currClassId,
                    Name = className,
                    Type = NodeType.Class,
                    Features = features
                });

                graph.Edges.Add(new Edge
                {
                    SourceId = currCompId,
                    TargetId = currClassId,
                    Type = EdgeType.ComponentContainsClass
                });

                graph.Edges.Add(new Edge
                {
                    SourceId = currClassId,
                    TargetId = currCompId,
                    Type = EdgeType.ClassContainedByComponent
                });

                var imports = new HashSet<string>();
                if (tomlTable.TryGetValue("imports", out var importsObj) && importsObj is TomlArray importsArray)
                {
                    foreach (var importItem in importsArray)
                    {
                        if (importItem != null)
                        {
                            imports.Add(importItem.ToString()!);
                        }
                    }
                }
                classDependencies[className] = imports;
            }

            foreach (var dirname in dirnames)
            {
                Walk(dirname);
            }
        }

        Walk(rootDirectory);

        var simpleNameToId = classNameToId.ToDictionary(
            kvp => kvp.Key.Split('.').Last(),
            kvp => kvp.Value);

        foreach (var kvp in classDependencies)
        {
            var srcClass = kvp.Key;
            var deps = kvp.Value;

            if (!classNameToId.TryGetValue(srcClass, out var srcId)) continue;

            foreach (var dstImport in deps)
            {
                var dstSimpleName = dstImport.Split('.').Last();
                if (simpleNameToId.TryGetValue(dstSimpleName, out var dstId) && srcId != dstId)
                {
                    graph.Edges.Add(new Edge
                    {
                        SourceId = srcId,
                        TargetId = dstId,
                        Type = EdgeType.ClassImportsClass
                    });
                }
            }
        }

        return graph;
    }

    private static double[] ExtractClassFeatures(TomlTable data)
    {
        double classLcom = 0.0;
        if (data.TryGetValue("lcom", out var lcomObj))
            classLcom = Convert.ToDouble(lcomObj);

        int numAttributes = 0;
        if (data.TryGetValue("attribute", out var attrObj) && attrObj is TomlArray attrArray)
            numAttributes = attrArray.Count;

        int numMethodsDeclared = 0;
        if (data.TryGetValue("num_methods", out var numMethObj))
            numMethodsDeclared = Convert.ToInt32(numMethObj);

        int numMethodsActual = 0;
        double totalMethodLcom = 0.0;
        int totalParams = 0;

        if (data.TryGetValue("methods", out var methodsObj) && methodsObj is TomlTableArray methodsArray)
        {
            numMethodsActual = methodsArray.Count;
            foreach (var methodItem in methodsArray)
            {
                if (methodItem.TryGetValue("lcom", out var mLcomObj))
                    totalMethodLcom += Convert.ToDouble(mLcomObj);

                if (methodItem.TryGetValue("params", out var mParamsObj) && mParamsObj is TomlArray paramsArray)
                    totalParams += paramsArray.Count;
            }
        }

        double avgMethodLcom = numMethodsActual > 0 ? totalMethodLcom / numMethodsActual : 0.0;
        double avgParams = numMethodsActual > 0 ? (double)totalParams / numMethodsActual : 0.0;

        return
        [
            classLcom,
            numAttributes,
            numMethodsDeclared,
            numMethodsActual,
            avgMethodLcom,
            avgParams
        ];
    }
}
