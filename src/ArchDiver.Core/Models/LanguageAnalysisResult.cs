using System.Collections.Generic;

namespace ArchDiver.Core.Models;

public class LanguageAnalysisResult
{
    public string Language { get; set; } = string.Empty;
    public List<ExtractedConcept> Methods { get; set; } = new();
    public List<ExtractedConcept> Classes { get; set; } = new();
    public List<ExtractedConcept> Fields { get; set; } = new();
    public List<ExtractedConcept> Imports { get; set; } = new();
    public List<ExtractedConcept> Identifiers { get; set; } = new();
}

public class ExtractedConcept
{
    public string Type { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public SourceRange Range { get; set; }
}
