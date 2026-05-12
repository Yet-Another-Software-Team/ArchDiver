using Microsoft.Extensions.Logging;
using ArchDiver.Core.Infrastructure;
using ArchDiver.Parser.Infrastructure;
using ArchDiver.Core.Models;
using ArchDiver.Core.Abstractions;
using Serilog;
using ArchDiver.Cli.Controllers;
using ArchDiver.Cli.Views;

namespace ArchDiver.Cli;

partial class Program
{
    private static readonly string _configFileName = "archdiver.toml";
    private static readonly string _outputDir = ".archdiver/out";
    private static ILoggerFactory _loggerFactory = null!;
    private static ICodeAnalysisEngine _analysisEngine = null!;
    private static readonly IConfigManager _configManager = new TomlConfigManager();

    public static IConfigManager ConfigManager => _configManager;


    static void Main(string[] args)
    {
        ProjectConfig config = File.Exists(_configFileName)
            ? ConfigManager.Load(_configFileName)
            : ConfigManager.GetDefault();
        ConfigureLogging(config);
        _analysisEngine = Bootstrapper.Initialize(_loggerFactory);
        var view = new ConsoleViewRenderer();
        var controllerLogger = _loggerFactory.CreateLogger<CommandController>();
        var controller = new CommandController(
            _loggerFactory,
            controllerLogger,
            view,
            _analysisEngine,
            ConfigManager,
            _configFileName,
            _outputDir);
        controller.Handle(args);
    }

    static void ConfigureLogging(ProjectConfig config)
    {
        var serilogLoggerConfiguration = new Serilog.LoggerConfiguration()
            .MinimumLevel.Is((Serilog.Events.LogEventLevel)config.Logging.MinimumLevel);

        if (!string.IsNullOrWhiteSpace(config.Logging.LogFilePath))
        {
            serilogLoggerConfiguration.WriteTo.File(config.Logging.LogFilePath);
        }

        Serilog.Log.Logger = serilogLoggerConfiguration.CreateLogger();

        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(Serilog.Log.Logger);
        });
    }
}
