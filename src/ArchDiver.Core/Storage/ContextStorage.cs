using ArchDiver.Core.Models;

namespace ArchDiver.Core.Storage;

/// <summary>
/// Serves as the centralized storage for maintaining state, parsed data,
/// and analysis results across the various components of the pipeline.
/// </summary>
public class ContextStorage
{
    // For now, it simply stores a collection of parsed AST nodes.
    public List<AstNode> ParsedAsts { get; private set; }

    public ContextStorage()
    {
        ParsedAsts = new List<AstNode>();
    }

    /// <summary>
    /// Stores an Abstract Syntax Tree node into the context.
    /// </summary>
    public void StoreAst(AstNode ast)
    {
        if (ast == null) throw new ArgumentNullException(nameof(ast));
        ParsedAsts.Add(ast);
    }

    /// <summary>
    /// Clears all data currently held in the context storage.
    /// </summary>
    public void Clear()
    {
        ParsedAsts.Clear();
    }
}
