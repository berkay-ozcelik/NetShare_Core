using System;
using NetShare_Core.Network;
using NetShare_Core.Entity;
using System.Net;
using NetShare_Core.Device;

namespace NetShare_Core.Protocol
{
    public interface RequestStrategy
    {
        public string HandleRequest(string payload);
       
    }

    public class IdentificationRequestStrategy : RequestStrategy
    {
        public string HandleRequest(string payload)
        {
            if(payload == string.Empty)
            {
                return Device.ComputerIdentifier.CurrentDevice.Serialize();
            }
            else
            {
            return "BAD_REQUEST";
            }
        }
    }

    public class FileListRequestStrategy : RequestStrategy
    {
        public string HandleRequest(string payload)
        {
            if(payload == string.Empty)
            {
                return Device.FileManager.Instance.Serialize();
            }
            else
            {
                return "BAD_REQUEST";
            }
        }
    }

    public class GetFileRequestStrategy : RequestStrategy
    {
        public string HandleRequest(string payload)
        {
            if(payload == string.Empty)
            {
                return "BAD_REQUEST";
            }
            
            SharingFile requestedFile = SharingFile.Deserialize(payload);
            
            SharingFile? actualFile = FileManager.Instance.GetFile(requestedFile);
            if (actualFile == null)
                return "BAD_REQUEST";
            
            
            string filePath = actualFile.FilePath;
            

            FileSenderSocket fileTransferSocket = new FileSenderSocket(filePath);
            
            return fileTransferSocket.Port.ToString();
        }
    }

}