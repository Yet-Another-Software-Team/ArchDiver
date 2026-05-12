using ArchDiver.Core.Models;
using ArchDiver.Shared.Models;

namespace ArchDiver.Core.Storage;

/// <summary>
/// Serves as the centralized storage for maintaining state, parsed data,
/// and analysis results across the various components of the pipeline.
/// </summary>
public class ContextStorage
{
    // Graph constructed from analysis artifacts (e.g., exported TOML).
    public Graph? CodeGraph { get; private set; }

    /// <summary>
    /// Stores a constructed code graph into the context.
    /// </summary>
    public void StoreGraph(Graph graph)
    {
        CodeGraph = graph ?? throw new ArgumentNullException(nameof(graph));
    }

    /// <summary>
    /// Clears all data currently held in the context storage.
    /// </summary>
    public void Clear()
    {
        CodeGraph = null;
    }
}
