using System.Collections.Generic;

namespace ArchDiver.GraphConstruction.Models;

public class Graph
{
    public List<Node> Nodes { get; set; } = [];
    public List<Edge> Edges { get; set; } = [];
}
