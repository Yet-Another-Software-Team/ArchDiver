using ArchDiver.Core.Abstractions;

namespace ArchDiver.Core.Abstractions;

public interface ILanguageRegistry
{
    void Register(ILanguageProvider provider);
    ILanguageProvider? Identify(string filePath, string content);
    ILanguageProvider? GetById(string languageId);
    IEnumerable<string> GetSupportedLanguages();
}
