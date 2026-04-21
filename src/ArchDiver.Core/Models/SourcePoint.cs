namespace ArchDiver.Core.Models;

/// <summary>
/// Represents a specific point in the source code.
/// </summary>
public record struct SourcePoint(int Line, int Column, int Offset);
