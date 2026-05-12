using System.Collections.Generic;

namespace ArchDiver.Parser.Abstractions;

/// <summary>
/// Defines the contract for a language plugin in the ArchDiver microkernel.
/// </summary>
public interface ILanguageProvider
{
    /// <summary>
    /// Unique identifier for the language.
    /// </summary>
    string LanguageId { get; }

    /// <summary>
    /// The base name of the native Tree-sitter library (without prefix or extension).
    /// </summary>
    string BaseLibraryName { get; }

    /// <summary>
    /// The name of the exported C function that returns the TSLanguage pointer.
    /// </summary>
    string FunctionName { get; }

    /// <summary>
    /// Mappings from semantic concepts to language-specific AST node types.
    /// </summary>
    IReadOnlyDictionary<string, string[]> NodeBindings { get; }

    /// <summary>
    /// Determines if this provider can handle the given file.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="content">The content of the file.</param>
    /// <returns>True if this provider can parse the file.</returns>
    bool CanHandle(string filePath, string content);
}
