using System;

namespace ArchDiver.Parser.Abstractions;

/// <summary>
/// Node binding attribute used to specify the concept and node types for a AST representation.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class NodeBindingAttribute(string concept, params string[] nodeTypes) : Attribute
{
    public string Concept { get; } = concept ?? throw new ArgumentNullException(nameof(concept));
    public string[] NodeTypes { get; } = nodeTypes ?? throw new ArgumentNullException(nameof(nodeTypes));
}
