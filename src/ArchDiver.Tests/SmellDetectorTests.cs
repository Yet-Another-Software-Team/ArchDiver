using System;
using System.Collections.Generic;
using ArchDiver.Shared.Models;
using ArchDiver.SmellAnalyzer;
using Xunit;

namespace ArchDiver.Tests;

public class SmellDetectorTests
{
    [Fact]
    public void AnalyzeGraph_WithAllEdgeTypes_ReturnsPredictions()
    {
        // Arrange
        var graph = new Graph();
        
        // Add 5 classes (Ids 0-4)
        for (int i = 0; i < 5; i++)
        {
            graph.Nodes.Add(new Node
            {
                Id = i,
                Type = NodeType.Class,
                Features = new double[] { 0, 0, 0, 0, 0, 0 }
            });
        }
        
        // Add 5 components (Ids 5-9)
        for (int i = 5; i < 10; i++)
        {
            graph.Nodes.Add(new Node
            {
                Id = i,
                Type = NodeType.Component,
                Features = new double[] { 0 }
            });
        }
        
        // edge_cc: Component -> Component (Id 5 -> Id 6)
        graph.Edges.Add(new Edge { SourceId = 5, TargetId = 6, Type = EdgeType.ComponentContainsComponent });

        // edge_cl: Component -> Class (Id 5 -> Id 0)
        graph.Edges.Add(new Edge { SourceId = 5, TargetId = 0, Type = EdgeType.ComponentContainsClass });

        // edge_cbc: Class -> Component (Id 1 -> Id 7)
        graph.Edges.Add(new Edge { SourceId = 1, TargetId = 7, Type = EdgeType.ClassContainedByComponent });

        // edge_ll: Class -> Class (Id 2 -> Id 3)
        graph.Edges.Add(new Edge { SourceId = 2, TargetId = 3, Type = EdgeType.ClassImportsClass });

        using var detector = new SmellDetector();

        // Act
        var predictions = detector.AnalyzeGraph(graph);

        // Assert
        Assert.NotNull(predictions);
        Assert.Equal(5, predictions.Count);
        Assert.True(predictions.ContainsKey(5));
        Assert.True(predictions.ContainsKey(7));
    }
}
