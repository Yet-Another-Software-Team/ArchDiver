using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using ArchDiver.Shared.Models;

namespace ArchDiver.SmellAnalyzer;

public class SmellDetector : IDisposable
{
    private readonly InferenceSession _session;
    private readonly string? _tempModelDir;

    public SmellDetector(string? modelPath = null)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            _tempModelDir = Path.Combine(Path.GetTempPath(), "ArchDiver", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempModelDir);

            string onnxPath = Path.Combine(_tempModelDir, "arch_smell_model.onnx");
            string dataPath = Path.Combine(_tempModelDir, "arch_smell_model.onnx.data");

            ExtractResource("Models.arch_smell_model.onnx", onnxPath);
            ExtractResource("Models.arch_smell_model.onnx.data", dataPath);

            modelPath = onnxPath;
        }

        _session = new InferenceSession(modelPath);
    }

    private static void ExtractResource(string resourceNameSuffix, string outputPath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string? resourceName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(resourceNameSuffix)) ?? throw new InvalidOperationException($"Resource ending with '{resourceNameSuffix}' not found. Available resources: {string.Join(", ", assembly.GetManifestResourceNames())}");

        using Stream? stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Failed to load resource '{resourceName}'.");

        using var fileStream = File.Create(outputPath);
        stream.CopyTo(fileStream);
    }

    /// <summary>
    /// Analyzes the graph for architectural smells using the ONNX model.
    /// Returns a dictionary mapping Node ID to smell probability/score.
    /// </summary>
    public Dictionary<int, float> AnalyzeGraph(Graph graph)
    {
        if (graph == null || graph.Nodes.Count == 0)
            return [];

        var classNodes = graph.Nodes.Where(n => n.Type == NodeType.Class).ToList();
        var compNodes = graph.Nodes.Where(n => n.Type == NodeType.Component).ToList();

        int classFeaturesLength = classNodes.FirstOrDefault()?.Features.Length ?? 6;
        int compFeaturesLength = compNodes.FirstOrDefault()?.Features.Length ?? 1;

        var xClass = new DenseTensor<float>(new[] { Math.Max(1, classNodes.Count), classFeaturesLength });
        var xComp = new DenseTensor<float>(new[] { Math.Max(1, compNodes.Count), compFeaturesLength });

        var classIdToIndex = new Dictionary<int, int>();
        for (int i = 0; i < classNodes.Count; i++)
        {
            classIdToIndex[classNodes[i].Id] = i;
            for (int j = 0; j < classFeaturesLength; j++)
                xClass[i, j] = (float)classNodes[i].Features[j];
        }

        var compIdToIndex = new Dictionary<int, int>();
        for (int i = 0; i < compNodes.Count; i++)
        {
            compIdToIndex[compNodes[i].Id] = i;
            for (int j = 0; j < compFeaturesLength; j++)
                xComp[i, j] = (float)compNodes[i].Features[j];
        }

        var edgesCC = graph.Edges.Where(e => e.Type == EdgeType.ComponentContainsComponent).ToList();
        var edgesCL = graph.Edges.Where(e => e.Type == EdgeType.ComponentContainsClass).ToList();
        var edgesCBC = graph.Edges.Where(e => e.Type == EdgeType.ClassContainedByComponent).ToList();
        var edgesLL = graph.Edges.Where(e => e.Type == EdgeType.ClassImportsClass).ToList();

        var edgeCC = new DenseTensor<long>(new[] { 2, Math.Max(1, edgesCC.Count) });
        var edgeCL = new DenseTensor<long>(new[] { 2, Math.Max(1, edgesCL.Count) });
        var edgeCBC = new DenseTensor<long>(new[] { 2, Math.Max(1, edgesCBC.Count) });
        var edgeLL = new DenseTensor<long>(new[] { 2, Math.Max(1, edgesLL.Count) });

        for (int i = 0; i < edgesCC.Count; i++)
        {
            if (compIdToIndex.TryGetValue(edgesCC[i].SourceId, out int srcIdx) && compIdToIndex.TryGetValue(edgesCC[i].TargetId, out int dstIdx))
            {
                edgeCC[0, i] = srcIdx;
                edgeCC[1, i] = dstIdx;
            }
        }

        for (int i = 0; i < edgesCL.Count; i++)
        {
            if (compIdToIndex.TryGetValue(edgesCL[i].SourceId, out int srcIdx) && classIdToIndex.TryGetValue(edgesCL[i].TargetId, out int dstIdx))
            {
                edgeCL[0, i] = srcIdx;
                edgeCL[1, i] = dstIdx;
            }
        }

        for (int i = 0; i < edgesCBC.Count; i++)
        {
            if (classIdToIndex.TryGetValue(edgesCBC[i].SourceId, out int srcIdx) && compIdToIndex.TryGetValue(edgesCBC[i].TargetId, out int dstIdx))
            {
                edgeCBC[0, i] = srcIdx;
                edgeCBC[1, i] = dstIdx;
            }
        }

        for (int i = 0; i < edgesLL.Count; i++)
        {
            if (classIdToIndex.TryGetValue(edgesLL[i].SourceId, out int srcIdx) && classIdToIndex.TryGetValue(edgesLL[i].TargetId, out int dstIdx))
            {
                edgeLL[0, i] = srcIdx;
                edgeLL[1, i] = dstIdx;
            }
        }

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("x_class", xClass),
            NamedOnnxValue.CreateFromTensor("x_comp", xComp),
            NamedOnnxValue.CreateFromTensor("edge_cc", edgeCC),
            NamedOnnxValue.CreateFromTensor("edge_cl", edgeCL),
            NamedOnnxValue.CreateFromTensor("edge_cbc", edgeCBC),
            NamedOnnxValue.CreateFromTensor("edge_ll", edgeLL)
        };

        using var results = _session.Run(inputs);

        var outputTensor = results.First().AsTensor<float>();

        var predictions = new Dictionary<int, float>();

        int maxOutputs = Math.Min(classNodes.Count, outputTensor.Dimensions[0]);
        for (int i = 0; i < maxOutputs; i++)
        {
            var nodeId = classNodes[i].Id;
            float logit = outputTensor.Dimensions.Length > 1 ? outputTensor[i, 0] : outputTensor[i];
            float score = 1.0f / (1.0f + (float)Math.Exp(-logit));
            predictions[nodeId] = score;
        }

        return predictions;
    }

    public void Dispose()
    {
        _session?.Dispose();
        if (_tempModelDir != null && Directory.Exists(_tempModelDir))
        {
            try { Directory.Delete(_tempModelDir, true); } catch { /* Ignore */ }
        }
    }
}
