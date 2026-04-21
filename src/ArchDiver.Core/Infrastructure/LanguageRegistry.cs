using System;
using System.Collections.Generic;
using System.Linq;
using ArchDiver.Core.Abstractions;

namespace ArchDiver.Core.Infrastructure;

/// <summary>
/// Central registry for language providers (The Kernel).
/// </summary>
public static class LanguageRegistry
{
    private static readonly List<ILanguageProvider> _providers = new();

    /// <summary>
    /// Registers a new language provider.
    /// </summary>
    public static void Register(ILanguageProvider provider)
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
    public static ILanguageProvider? Identify(string filePath, string content)
    {
        return _providers.FirstOrDefault(p => p.CanHandle(filePath, content));
    }

    /// <summary>
    /// Gets a provider by its unique ID.
    /// </summary>
    public static ILanguageProvider? GetById(string languageId)
    {
        return _providers.FirstOrDefault(p => p.LanguageId.Equals(languageId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Lists all registered languages.
    /// </summary>
    public static IEnumerable<string> GetSupportedLanguages() => _providers.Select(p => p.LanguageId);
}
