using ArchDiver.Core.Abstractions;
using ArchDiver.Cli.Views;
using Microsoft.Extensions.Logging;

namespace ArchDiver.Cli.Controllers;

public partial class CommandController(
    ILoggerFactory loggerFactory,
    ILogger<CommandController> logger,
    IViewRenderer view,
    ICodeAnalysisEngine analysisEngine,
    IConfigManager configManager,
    string configFileName,
    string outputDir)
{
    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private readonly ILogger<CommandController> _logger = logger;
    private readonly IViewRenderer _view = view;
    private readonly ICodeAnalysisEngine _analysisEngine = analysisEngine;
    private readonly IConfigManager _configManager = configManager;
    private readonly string _configFileName = configFileName;
    private readonly string _outputDir = outputDir;

    public void Handle(string[] args)
    {
        var config = LoadConfig();
        if (args.Length < 1)
        {
            _view.ShowUsage(_analysisEngine.GetSupportedLanguages());
            return;
        }

        string command = args[0].ToLower();
        switch (command)
        {
            case "explore":
                HandleExplore(args, config);
                break;
            case "analyze":
                HandleAnalyze(args, config);
                break;
            case "config":
                HandleConfig(args, config);
                break;
            default:
                _view.ShowUsage(_analysisEngine.GetSupportedLanguages());
                break;
        }
    }

}
