namespace ArchDiver.Core.Models;

/// <summary>
/// Represents a range within the source code.
/// </summary>
public record struct SourceRange(SourcePoint Start, SourcePoint End);
