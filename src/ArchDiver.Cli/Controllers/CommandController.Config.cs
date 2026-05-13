using ArchDiver.Core.Models;

namespace ArchDiver.Cli.Controllers;

public partial class CommandController
{
    public ProjectConfig LoadConfig()
        => File.Exists(_configFileName)
            ? _configManager.Load(_configFileName)
            : _configManager.GetDefault();

    private int HandleConfig(string[] args, ProjectConfig config)
    {
        if (args.Length > 1 && args[1].Equals("create", StringComparison.CurrentCultureIgnoreCase))
        {
            _configManager.Save(_configManager.GetDefault(), _configFileName);
            LogConfigCreated(_logger, _configFileName);
            return 0;
        }

        if (!File.Exists(_configFileName))
        {
            LogNoConfigFound(_logger);
            _view.ShowConfig(_configManager.GetDefault(), _configFileName);
        }
        else
        {
            _view.ShowConfig(_configManager.Load(_configFileName), _configFileName);
        }

        return 0;
    }
}
