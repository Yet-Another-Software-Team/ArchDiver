using ArchDiver.Core.Models;

namespace ArchDiver.Cli.Controllers;

public partial class CommandController
{
    public ProjectConfig LoadConfig()
        => File.Exists(_configFileName)
            ? _configManager.Load(_configFileName)
            : _configManager.GetDefault();

    public ProjectConfig ResolveConfig(string? path)
    {
        if (string.IsNullOrEmpty(path)) return LoadConfig();

        string? nearest = _configManager.FindConfig(path);
        return nearest != null ? _configManager.Load(nearest) : _configManager.GetDefault();
    }

    private int HandleConfig(string[] args, ProjectConfig config)
    {
        // Handle 'config create [path]'
        if (args.Length > 1 && args[1].Equals("create", StringComparison.CurrentCultureIgnoreCase))
        {
            string targetPath = _configFileName;
            if (args.Length > 2)
            {
                targetPath = args[2];
                // If the provided path is a directory, append the default config filename
                if (Directory.Exists(targetPath))
                {
                    targetPath = Path.Combine(targetPath, "archdiver.toml");
                }
            }
            _configManager.Save(_configManager.GetDefault(), targetPath);
            LogConfigCreated(_logger, targetPath);
            return 0;
        }

        string configToShowPath = _configFileName;
        ProjectConfig configToShow = config;

        // Handle 'config [path]'
        if (args.Length > 1)
        {
            string requestedPath = args[1];
            if (Directory.Exists(requestedPath))
            {
                string? nearest = _configManager.FindConfig(requestedPath);
                if (nearest != null)
                {
                    configToShowPath = nearest;
                    configToShow = _configManager.Load(nearest);
                }
                else
                {
                    configToShowPath = Path.Combine(requestedPath, "archdiver.toml (not found)");
                    configToShow = _configManager.GetDefault();
                }
            }
            else if (File.Exists(requestedPath))
            {
                configToShowPath = requestedPath;
                configToShow = _configManager.Load(requestedPath);
            }
            else
            {
                // Path not found, fallback to defaults or show warning
                LogPathNotFound(_logger, requestedPath);
                return 1;
            }
        }

        if (configToShowPath.Contains("(not found)") || !File.Exists(configToShowPath))
        {
            if (args.Length == 1) LogNoConfigFound(_logger);
            _view.ShowConfig(configToShow, configToShowPath);
        }
        else
        {
            _view.ShowConfig(configToShow, configToShowPath);
        }

        return 0;
    }
}
