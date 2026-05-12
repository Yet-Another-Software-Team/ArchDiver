using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using ArchDiver.Shared.Models;

namespace ArchDiver.SmellAnalyzer;

public class SmellDetector : IDisposable
{
    private readonly InferenceSession _session;

    public SmellDetector(string? modelPath = null)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            var assemblyPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            modelPath = System.IO.Path.Combine(assemblyPath!, "Models", "code_smell_model.onnx");
        }

        _session = new InferenceSession(modelPath);
    }

    /// <summary>
    /// Analyzes the graph for architectural smells using the ONNX model.
    /// Returns a dictionary mapping Node ID to smell probability/score.
    /// </summary>
    public Dictionary<int, float> AnalyzeGraph(Graph graph)
    {
        if (graph == null || graph.Nodes.Count == 0)
            return new Dictionary<int, float>();

        // We have heterogeneous nodes: Class and Component
        var classNodes = graph.Nodes.Where(n => n.Type == NodeType.Class).ToList();
        var compNodes = graph.Nodes.Where(n => n.Type == NodeType.Component).ToList();

        int classFeaturesLength = classNodes.FirstOrDefault()?.Features.Length ?? 6;
        int compFeaturesLength = compNodes.FirstOrDefault()?.Features.Length ?? 1;

        // Features Tensors
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

        // We have heterogeneous edges:
        // edge_cc: Component -> Component
        // edge_cl: Component -> Class
        // edge_ll: Class -> Class (Imports)

        var edgesCC = graph.Edges.Where(e => e.Type == EdgeType.ComponentContainsComponent).ToList();
        var edgesCL = graph.Edges.Where(e => e.Type == EdgeType.ComponentContainsClass).ToList();
        var edgesLL = graph.Edges.Where(e => e.Type == EdgeType.ClassImportsClass).ToList();

        var edgeCC = new DenseTensor<long>(new[] { 2, Math.Max(1, edgesCC.Count) });
        var edgeCL = new DenseTensor<long>(new[] { 2, Math.Max(1, edgesCL.Count) });
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
            NamedOnnxValue.CreateFromTensor("edge_ll", edgeLL)
        };

        using var results = _session.Run(inputs);

        var outputTensor = results.First().AsTensor<float>();

        var predictions = new Dictionary<int, float>();

        // Output presumably maps back to classes? The error might occur here depending on the shape of the output.
        // Let's assume output maps to classes for now
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
    }
}
