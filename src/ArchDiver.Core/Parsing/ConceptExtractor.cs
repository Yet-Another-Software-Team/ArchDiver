using System;
using System.Collections.Generic;
using System.Linq;
using ArchDiver.Core.Abstractions;
using ArchDiver.Core.Models;

namespace ArchDiver.Core.Parsing
{
    public class ConceptExtractor
    {
        private readonly ILanguageProvider _provider;

        public ConceptExtractor(ILanguageProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public LanguageAnalysisResult Extract(AstNode root)
        {
            var result = new LanguageAnalysisResult
            {
                Language = _provider.LanguageId
            };

            Traverse(root, result);

            return result;
        }

        private void Traverse(AstNode node, LanguageAnalysisResult result)
        {
            foreach (var binding in _provider.NodeBindings)
            {
                if (binding.Value.Contains(node.Type))
                {
                    var concept = new ExtractedConcept
                    {
                        Type = node.Type,
                        Text = node.Text,
                        Range = node.Range
                    };

                    switch (binding.Key)
                    {
                        case "Method":
                            result.Methods.Add(concept);
                            break;
                        case "Class":
                            result.Classes.Add(concept);
                            break;
                        case "Field":
                            result.Fields.Add(concept);
                            break;
                        case "Import":
                            result.Imports.Add(concept);
                            break;
                        case "Identifier":
                            result.Identifiers.Add(concept);
                            break;
                    }
                }
            }

            foreach (var child in node.Children)
            {
                Traverse(child, result);
            }
        }
    }
}
