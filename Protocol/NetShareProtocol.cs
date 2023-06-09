using System.Text.Json;
using NetShare_Core.Device;
using NetShare_Core.Entity;

namespace NetShare_Core.Protocol
{

    public class NetShareRequest
    {
        public NetShareProtocolHeader Header { get; set; }
        public string Payload { get; set; }

        public NetShareRequest(NetShareProtocolHeader header, string payload)
        {
            Header = header;
            Payload = payload;
        }
        
        public string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }

        public static NetShareRequest Deserialize(string json)
        {
            return JsonSerializer.Deserialize<NetShareRequest>(json);
        }
    }

    public enum NetShareProtocolHeader
    {
        IDENTIFICATION,
        FILE_LIST,
        GET_FILE
    }

    public static class NetShareProtocol
    {   
        
        private static RequestStrategy _identificationRequestStrategy;
        private static RequestStrategy _fileListRequestStrategy;
        private static RequestStrategy _getFileRequestStrategy;

        static NetShareProtocol()
        {
            _identificationRequestStrategy = new IdentificationRequestStrategy();
            _fileListRequestStrategy = new FileListRequestStrategy();
            _getFileRequestStrategy = new GetFileRequestStrategy();
        }

        public static string IdentificationRequest()
        {
            return new NetShareRequest(
                NetShareProtocolHeader.IDENTIFICATION, 
                string.Empty).Serialize();
        }

        public static DeviceInfo IdentificationResponse(string response)
        {
            return DeviceInfo.Deserialize(response);
        }

        public static string FileListRequest()
        {
            return new NetShareRequest(
                NetShareProtocolHeader.FILE_LIST, 
                string.Empty).Serialize();
        }

        public static List<SharingFile> FileListResponse(string response)
        {
            return FileManager.Deserialize(response);
        }

        public static string GetFileRequest(SharingFile requestedFile)
        {
            return new NetShareRequest(
                NetShareProtocolHeader.GET_FILE, 
                requestedFile.Serialize()).Serialize();
        }

        public static int GetFileResponse(string response)
        {
            return int.Parse(response);
        }


        public static string GenerateResponse(string request)
        {   

            var requestObject = NetShareRequest.Deserialize(request);


            NetShareProtocolHeader requestHeader = requestObject.Header;
            string payload = requestObject.Payload;

            
            switch(requestHeader)
            {
                case NetShareProtocolHeader.IDENTIFICATION:
                    return _identificationRequestStrategy.HandleRequest(payload);
                case NetShareProtocolHeader.FILE_LIST:
                    return _fileListRequestStrategy.HandleRequest(payload);
                case NetShareProtocolHeader.GET_FILE:
                    return _getFileRequestStrategy.HandleRequest(payload);
                default:
                    return "BAD_REQUEST";
            }
        }
    }
}
