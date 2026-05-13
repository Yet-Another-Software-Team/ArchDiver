using Microsoft.Extensions.Logging;

namespace ArchDiver.Cli.Controllers;

public partial class CommandController
{
    private static void LogConfigCreated(ILogger logger, string configFileName)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Created default configuration: {ConfigFileName}", configFileName);
        }
    }

    private static void LogNoConfigFound(ILogger logger)
    {
        if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning("No config file found. Using defaults.");
        }
    }

    private static void LogMissingDirectory(ILogger logger)
    {
        if (logger.IsEnabled(LogLevel.Error))
        {
            logger.LogError("Missing directory path.");
        }
    }

    private static void LogDirectoryNotFound(ILogger logger, string rootPath)
    {
        if (logger.IsEnabled(LogLevel.Error))
        {
            logger.LogError("Directory not found: {RootPath}", rootPath);
        }
    }

    private static void LogModelNotFound(ILogger logger, string modelPath)
    {
        if (logger.IsEnabled(LogLevel.Error))
        {
            logger.LogError("Model not found: {ModelPath}", modelPath);
        }
    }

    private static void LogExploring(ILogger logger, string rootPath, int maxDepth)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Exploring: {RootPath} (Max Depth: {MaxDepth})", rootPath, maxDepth);
        }
    }

    private static void LogExplorationComplete(ILogger logger, string outputRoot)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Exploration complete. Results saved in {OutputRoot}", outputRoot);
        }
    }

    private static void LogAnalyzingWithModel(ILogger logger, string rootPath, int maxDepth, string modelPath)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Analyzing: {RootPath} (Max Depth: {MaxDepth}) with model {ModelPath}", rootPath, maxDepth, modelPath);
        }
    }

    private static void LogAnalyzingWithBuiltInModel(ILogger logger, string rootPath, int maxDepth)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Analyzing: {RootPath} (Max Depth: {MaxDepth}) with built-in model", rootPath, maxDepth);
        }
    }
}
