using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ArchDiver.Core.Models;

/// <summary>
/// Represents a generic node in the Abstract Syntax Tree (AST).
/// </summary>
public class AstNode
{
    public string Type { get; set; } = string.Empty;
    public string? FieldName { get; set; }
    public string Text { get; set; } = string.Empty;
    public SourceRange Range { get; set; }
    [JsonIgnore]
    public AstNode? Parent { get; set; }
    [Tomlyn.Serialization.TomlPropertyOrder(100)]
    public List<AstNode> Children { get; set; } = new List<AstNode>();
    /// <summary>
    /// Adds a child node and sets its parent reference.
    /// </summary>
    public void AddChild(AstNode child)

    {
        if (child == null) throw new ArgumentNullException(nameof(child));
        child.Parent = this;
        Children.Add(child);
    }
}
