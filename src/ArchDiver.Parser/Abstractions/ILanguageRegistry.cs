using System.Collections.Generic;

namespace ArchDiver.Parser.Abstractions;

public interface ILanguageRegistry
{
    void Register(ILanguageProvider provider);
    ILanguageProvider? Identify(string filePath, string content);
    ILanguageProvider? GetById(string languageId);
    IEnumerable<string> GetSupportedLanguages();
}
