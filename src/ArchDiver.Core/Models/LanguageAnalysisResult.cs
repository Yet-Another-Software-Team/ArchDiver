namespace ArchDiver.Core.Models;

public class FileAnalysisResult
{
    public List<ComponentResult> Components { get; set; } = [];
    public List<string> Imports { get; set; } = [];
}

public class ComponentResult
{
    public string Name { get; set; } = string.Empty;
    public List<string> Attribute { get; set; } = [];
    public List<MethodResult> Methods { get; set; } = [];
    public int NumMethods => Methods.Count;
}

public class MethodResult
{
    public string Name { get; set; } = string.Empty;
    public List<string> Params { get; set; } = [];
    public double Lcom { get; set; }
}
