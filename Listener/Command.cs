using System.Text.Json;
using NetShare_Core.Entity;

namespace NetShare_Core.Listener
{
    public static class CommandContext
    {
        private static Dictionary<string, Command> _commands;
        private static Command _errorCommand;


        static CommandContext()
        {
            _commands = new Dictionary<string, Command>
            {
                { "StartAcceptor", new StartAcceptorCommand() },
                { "DiscoverDevices", new DiscoverDevicesCommand() },
                { "SelectDevice", new SelectDeviceCommand() },
                { "GetSharingFiles", new GetSharingFilesCommand() },
                { "SelectFile", new SelectFileCommand() },
                { "SetDownloadDirectory", new SetDownloadDirectoryCommand() },
                { "StartDownload", new StartDownloadCommand() },
                { "GetDownloadProgress", new GetDownloadProgressCommand() },
                { "StopDownload", new StopDownloadCommand() },
                { "ShareFile", new ShareFileCommand() },
                { "StopShareFile", new StopShareFileCommand() },
                { "GetSharingFilesOfCurrentDevice", new GetSharingFilesOfCurrentDeviceCommand() }
            };

            _errorCommand = new ErrorCommand();
        }

        public static Command GetCommand(string rawRequest)
        {
            try
            {
                CommandRequest request = JsonSerializer.Deserialize<CommandRequest>(rawRequest);
                Command command = _commands[request.CommandName];
                command.Parameter = request.Parameter;
                return command;
            }
            catch
            {
                return _errorCommand;
            }

        }
    }




    public abstract class Command
    {
        private string _parameter;

        public Command()
        {
            _parameter = string.Empty;
        }

        public abstract CommandResult Execute();

        public string Parameter
        {
            get => _parameter;
            set => _parameter = value;
        }
    }

    public class StartAcceptorCommand : Command
    {
        public override CommandResult Execute()
        {
            try
            {
                NetShare.Facade.StartAcceptor();
                return CommandResult.Success();
            }
            catch (Exception e)
            {
                return CommandResult.Error(e.Message);
            }
        }
    }

    public class DiscoverDevicesCommand : Command
    {
        public override CommandResult Execute()
        {
            try
            {
                var devices = NetShare.Facade.DiscoverDevices();
                return CommandResult.Success(JsonSerializer.Serialize(devices));
            }
            catch (Exception e)
            {
                return CommandResult.Error(e.Message);
            }
        }
    }

    public class SelectDeviceCommand : Command
    {
        public override CommandResult Execute()
        {
            try
            {
                int index = JsonSerializer.Deserialize<int>(Parameter);
                NetShare.Facade.SelectDevice(index);
                return CommandResult.Success();
            }
            catch (Exception e)
            {
                return CommandResult.Error(e.Message);
            }
        }
    }

    public class GetSharingFilesCommand : Command
    {
        public override CommandResult Execute()
        {
            try
            {
                var files = NetShare.Facade.GetSharingFiles();
                return CommandResult.Success(JsonSerializer.Serialize(files));
            }
            catch (Exception e)
            {
                return CommandResult.Error(e.Message);
            }
        }
    }

    public class SelectFileCommand : Command
    {
        public override CommandResult Execute()
        {
            try
            {
                int index = JsonSerializer.Deserialize<int>(Parameter);
                NetShare.Facade.SelectFile(index);
                return CommandResult.Success();
            }
            catch (Exception e)
            {
                return CommandResult.Error(e.Message);
            }
        }
    }

    public class SetDownloadDirectoryCommand : Command
    {
        public override CommandResult Execute()
        {
            try
            {
                string path = Parameter;
                NetShare.Facade.SetDownloadDirectory(path);
                return CommandResult.Success();
            }
            catch (Exception e)
            {
                return CommandResult.Error(e.Message);
            }
        }
    }

    public class StartDownloadCommand : Command
    {
        public override CommandResult Execute()
        {
            try
            {
                int downloadId = NetShare.Facade.StartDownload();
                return CommandResult.Success(JsonSerializer.Serialize(downloadId));
            }
            catch (Exception e)
            {
                return CommandResult.Error(e.Message);
            }
        }
    }

    public class GetDownloadProgressCommand : Command
    {
        public override CommandResult Execute()
        {
            try
            {
                int downloadId = JsonSerializer.Deserialize<int>(Parameter);
                int progress = NetShare.Facade.GetDownloadProgress(downloadId);
                return CommandResult.Success(JsonSerializer.Serialize(progress));
            }
            catch (Exception e)
            {
                return CommandResult.Error(e.Message);
            }
        }
    }

    public class StopDownloadCommand : Command
    {
        public override CommandResult Execute()
        {
            try
            {
                int downloadId = JsonSerializer.Deserialize<int>(Parameter);
                NetShare.Facade.StopDownload(downloadId);
                return CommandResult.Success();
            }
            catch (Exception e)
            {
                return CommandResult.Error(e.Message);
            }
        }
    }

    public class ShareFileCommand : Command
    {
        public override CommandResult Execute()
        {
            try
            {
                string path = Parameter;
                NetShare.Facade.ShareFile(path);
                return CommandResult.Success();
            }
            catch (Exception e)
            {
                return CommandResult.Error(e.Message);
            }
        }
    }

    public class StopShareFileCommand : Command
    {
        public override CommandResult Execute()
        {
            try
            {
                int index = JsonSerializer.Deserialize<int>(Parameter);
                NetShare.Facade.StopShareFile(index);
                return CommandResult.Success();
            }
            catch (Exception e)
            {
                return CommandResult.Error(e.Message);
            }
        }
    }

    public class GetSharingFilesOfCurrentDeviceCommand : Command
    {
        public override CommandResult Execute()
        {
            try
            {
                var files = NetShare.Facade.GetSharingFilesOfCurrentDevice();
                return CommandResult.Success(JsonSerializer.Serialize(files));
            }
            catch (Exception e)
            {
                return CommandResult.Error(e.Message);
            }
        }
    }

    public class ErrorCommand : Command
    {
        public override CommandResult Execute()
        {
            return CommandResult.Error("Command not found");
        }
    }

}
