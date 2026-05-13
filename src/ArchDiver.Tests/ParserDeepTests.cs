using ArchDiver.Core.Models;
using ArchDiver.Parser.Parsing;
using ArchDiver.Parser.Languages;
using ArchDiver.Parser.Abstractions;
using ArchDiver.Parser.Infrastructure;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace ArchDiver.Tests;

public class ParserDeepTests
{
    private readonly ILanguageRegistry _registry;

    public ParserDeepTests()
    {
        _registry = new LanguageRegistry();
        _registry.Register(new CSharpLanguageProvider());
        _registry.Register(new PythonLanguageProvider());
        _registry.Register(new JavaLanguageProvider());
    }

    [Fact]
    public void Analyze_CSharpClass_ExtractsNameAndMethods()
    {
        // Arrange
        var engine = new ParserEngine(_registry, NullLoggerFactory.Instance);
        var sourceCode = @"
using System;
namespace Test {
    class MyClass {
        private int _field;
        public void Method1() { _field = 1; }
        public void Method2() { Console.WriteLine(_field); }
    }
}";
        // Act
        var result = engine.Analyze(sourceCode, "Test.cs");

        // Assert
        Assert.Single(result.Components);
        var comp = result.Components[0];
        Assert.Equal("MyClass", comp.Name);
        Assert.Contains("Method1", comp.Methods.Select(m => m.Name));
        Assert.Contains("Method2", comp.Methods.Select(m => m.Name));
        Assert.Contains("_field", comp.Attribute);
    }

    [Fact]
    public void Analyze_PythonModule_ExtractsGlobalFunctions()
    {
        // Arrange
        var engine = new ParserEngine(_registry, NullLoggerFactory.Instance);
        var sourceCode = @"
def func1():
    pass

def func2():
    pass
";
        // Act
        var result = engine.Analyze(sourceCode, "test.py");

        // Assert
        Assert.Single(result.Components);
        var comp = result.Components[0];
        Assert.Equal("GlobalModule", comp.Name);
        Assert.Equal(2, comp.Methods.Count);
    }
}
