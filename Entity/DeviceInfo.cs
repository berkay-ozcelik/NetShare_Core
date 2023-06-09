using System.Text.Json;
namespace NetShare_Core.Entity
{
    public class DeviceInfo
    {


        public string DeviceName { get; set; }
        public string DeviceOS { get; set; }
        public string EndPoint { get; set; }

        public DeviceInfo(string deviceName, string deviceOS)
        {
            DeviceName = deviceName;
            DeviceOS = deviceOS;
            EndPoint = string.Empty;
        }

        public string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }

        public static DeviceInfo Deserialize(string json)
        {
            return JsonSerializer.Deserialize<DeviceInfo>(json);
        }

        
        public override string ToString()
        {
            return $"{DeviceName} ({DeviceOS})";
        }

    }

}
        