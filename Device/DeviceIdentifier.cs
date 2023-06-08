using NLog;
using NetShare_Core.Entity;

namespace NetShare_Core.Device
{
    public static class ComputerIdentifier
    {   
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public static readonly DeviceInfo CurrentDevice = GetDeviceInfo();

        private static DeviceInfo GetDeviceInfo()
        {   

            var deviceName = System.Environment.MachineName;
            var deviceOS = System.Environment.OSVersion.ToString();

            logger.Info("NetShare_Core is running on {0} with OS {1}", deviceName, deviceOS);

            return new DeviceInfo(deviceName, deviceOS);
        }
    }
}