namespace ArchDiver.Shared.Models;

public class Edge
{
    public int SourceId { get; set; }
    public int TargetId { get; set; }
    public EdgeType Type { get; set; }
}
