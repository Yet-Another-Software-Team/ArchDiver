using System.Collections.Generic;

namespace ArchDiver.Core.Models;

public class FileAnalysisResult
{
    public List<ComponentResult> Components { get; set; } = new();
    public List<string> Imports { get; set; } = new();
}

public class ComponentResult
{
    public string Name { get; set; } = string.Empty;
    public List<string> Attribute { get; set; } = new();
    public List<MethodResult> Methods { get; set; } = new();
    public int NumMethods => Methods.Count;
}

public class MethodResult
{
    public string Name { get; set; } = string.Empty;
    public List<string> Params { get; set; } = new();
    public double Lcom { get; set; }
}
