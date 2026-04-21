namespace ArchDiver.Core.Abstractions;

/// <summary>
/// Node binding attribute used to specify the concept and node types for a AST representation.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class NodeBindingAttribute : Attribute
{
    public string Concept { get; }
    public string[] NodeTypes { get; }

    public NodeBindingAttribute(string concept, params string[] nodeTypes)
    {
        Concept = concept ?? throw new ArgumentNullException(nameof(concept));
        NodeTypes = nodeTypes ?? throw new ArgumentNullException(nameof(nodeTypes));
    }
}
