using ArchDiver.Core.Languages;
using ArchDiver.Core.Abstractions;

namespace ArchDiver.Core.Infrastructure;

/// <summary>
/// Handles the initialization and registration of all plugins in the microkernel.
/// </summary>
public static class Bootstrapper
{
    public static ILanguageRegistry Initialize()
    {
        var registry = new LanguageRegistry();

        // Register core language providers
        registry.Register(new CSharpLanguageProvider());
        registry.Register(new PythonLanguageProvider());
        registry.Register(new JavaLanguageProvider());

        return registry;
    }
}
