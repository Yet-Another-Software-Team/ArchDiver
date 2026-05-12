using ArchDiver.Parser.Abstractions;

namespace ArchDiver.Parser.Infrastructure;

/// <summary>
/// Registry for language providers.
/// </summary>
public class LanguageRegistry : ILanguageRegistry
{
    private readonly List<ILanguageProvider> _providers = new();

    /// <summary>
    /// Registers a new language provider.
    /// </summary>
    public void Register(ILanguageProvider provider)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));

        if (!_providers.Any(p => p.LanguageId == provider.LanguageId))
        {
            _providers.Add(provider);
        }
    }

    /// <summary>
    /// Identifies the correct provider for a given file.
    /// </summary>
    public ILanguageProvider? Identify(string filePath, string content)
    {
        return _providers.FirstOrDefault(p => p.CanHandle(filePath, content));
    }

    /// <summary>
    /// Gets a provider by its unique ID.
    /// </summary>
    public ILanguageProvider? GetById(string languageId)
    {
        return _providers.FirstOrDefault(p => p.LanguageId.Equals(languageId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Lists all registered languages.
    /// </summary>
    public IEnumerable<string> GetSupportedLanguages() => _providers.Select(p => p.LanguageId);
}
