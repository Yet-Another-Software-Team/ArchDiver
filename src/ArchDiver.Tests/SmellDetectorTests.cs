using System;
using System.Collections.Generic;
using ArchDiver.Shared.Models;
using ArchDiver.SmellAnalyzer;
using Xunit;

namespace ArchDiver.Tests;

public class SmellDetectorTests
{
    [Fact]
    public void AnalyzeGraph_WithMoreComponentsThanClasses_DoesNotThrow()
    {
        // Arrange
        var graph = new Graph();
        
        // Add 10 classes (Indices 0-9)
        for (int i = 0; i < 10; i++)
        {
            graph.Nodes.Add(new Node
            {
                Id = i,
                Type = NodeType.Class,
                Features = new double[] { 0, 0, 0, 0, 0, 0 }
            });
        }
        
        // Add 20 components (Indices 10-29)
        for (int i = 10; i < 30; i++)
        {
            graph.Nodes.Add(new Node
            {
                Id = i,
                Type = NodeType.Component,
                Features = new double[] { 0 }
            });
        }
        
        // Add a ClassContainedByComponent edge
        // Currently mapped to edge_ll (which model likely thinks is L->L)
        // SourceId = 0 (Class), TargetId = 29 (Component)
        graph.Edges.Add(new Edge
        {
            SourceId = 0,
            TargetId = 29,
            Type = EdgeType.ClassContainedByComponent
        });

        using var detector = new SmellDetector();

        // Act & Assert
        // This is expected to throw before the fix
        var exception = Record.Exception(() => detector.AnalyzeGraph(graph));
        Assert.Null(exception);
    }
}
