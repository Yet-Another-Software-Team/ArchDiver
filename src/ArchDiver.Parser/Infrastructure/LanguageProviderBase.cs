using System.Collections.ObjectModel;
using System.Reflection;
using ArchDiver.Parser.Abstractions;

namespace ArchDiver.Parser.Infrastructure;

public abstract class LanguageProviderBase : ILanguageProvider
{
    public abstract string LanguageId { get; }
    public abstract string BaseLibraryName { get; }
    public abstract string FunctionName { get; }

    private IReadOnlyDictionary<string, string[]>? _nodeBindings;
    /// <summary>
    /// Binds attributes to node types for the AST representation of the code.
    /// </summary>
    public virtual IReadOnlyDictionary<string, string[]> NodeBindings
    {
        get
        {
            if (_nodeBindings == null)
            {
                _nodeBindings = InitializeNodeBindings();
            }
            return _nodeBindings;
        }
    }

    protected virtual IReadOnlyDictionary<string, string[]> InitializeNodeBindings()
    {
        var bindings = new Dictionary<string, string[]>();
        var attributes = GetType().GetCustomAttributes<NodeBindingAttribute>();
        foreach (var attr in attributes)
        {
            bindings[attr.Concept] = attr.NodeTypes;
        }
        return new ReadOnlyDictionary<string, string[]>(bindings);
    }

    public abstract bool CanHandle(string filePath, string content);
}
