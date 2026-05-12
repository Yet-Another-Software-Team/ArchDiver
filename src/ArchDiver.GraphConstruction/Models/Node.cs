using System;

namespace ArchDiver.GraphConstruction.Models;

public class Node
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public NodeType Type { get; set; }
    public double[] Features { get; set; } = [];
}
