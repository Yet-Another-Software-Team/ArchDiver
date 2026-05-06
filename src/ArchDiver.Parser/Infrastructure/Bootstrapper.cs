using ArchDiver.Parser.Languages;
using ArchDiver.Parser.Abstractions;
using ArchDiver.Core.Abstractions;
using ArchDiver.Parser.Parsing;
using Microsoft.Extensions.Logging;

namespace ArchDiver.Parser.Infrastructure;

/// <summary>
/// Handles the initialization and registration of all plugins in the microkernel.
/// </summary>
public static class Bootstrapper
{
    public static ICodeAnalysisEngine Initialize(ILoggerFactory loggerFactory)
    {
        var registry = new LanguageRegistry();

        // Register core language providers
        registry.Register(new CSharpLanguageProvider());
        registry.Register(new PythonLanguageProvider());
        registry.Register(new JavaLanguageProvider());

        return new ParserEngine(registry, loggerFactory);
    }
}
