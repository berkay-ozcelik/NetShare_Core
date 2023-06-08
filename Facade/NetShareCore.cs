using System;
using NetShare_Core.Network;
using NetShare_Core.Entity;
using NetShare_Core.Device;
using System.Net;
using NetShare_Core.Protocol;

namespace NetShare
{

    public class Facade
    {   
        private static Facade _instance;

        public static Facade Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new Facade();
                }
                return _instance;
            }
        }

        private Acceptor _acceptor;
        private DeviceInfo[] _devices;
        private int _selectedDeviceIndex;
        private List<SharingFile> _sharingFilesOfSelectedDevice;
        private int _selectedFileIndex;

        private string _downloadDirectory;

        private Facade()
        {
            
        }
        
        public void StartAcceptor()
        {
            _acceptor = new Acceptor();
        }

        public void DiscoverDevices()
        {
            _devices = Requestor.Discover();

            // Print devices with index
            for (int i = 0; i < _devices.Length; i++)
            {
                Console.WriteLine($"[{i}] {_devices[i]}");
            }
        }

        public void SelectDevice(int index)
        {
            _selectedDeviceIndex = index;
        }

        public void GetSharingFiles()
        {
            var selectedDevice = _devices[_selectedDeviceIndex];
           
            var tcpServerEndpoint = IPEndPoint.Parse(selectedDevice.EndPoint); 
 
            var response = Requestor.SendRequest(
                        NetShareProtocol.FileListRequest(), 
                        tcpServerEndpoint);

            _sharingFilesOfSelectedDevice = NetShareProtocol.FileListResponse(response);

            // Print files with index
            for (int i = 0; i < _sharingFilesOfSelectedDevice.Count; i++)
            {
                Console.WriteLine($"[{i}] {_sharingFilesOfSelectedDevice[i]}");
            }
        }

        public void SelectFile(int index)
        {
            _selectedFileIndex = index;
        }

        public void SetDownloadDirectory(string directory)
        {
            _downloadDirectory = directory;
        }

        public void DownloadFile()
        {
            var selectedDevice = _devices[_selectedDeviceIndex];
            var selectedFile = _sharingFilesOfSelectedDevice[_selectedFileIndex];

            var tcpServerEndpoint = IPEndPoint.Parse(selectedDevice.EndPoint); 

            var fileDownloadRequest = NetShareProtocol.GetFileRequest(selectedFile);

            var response = Requestor.SendRequest(fileDownloadRequest, tcpServerEndpoint);

            var port = NetShareProtocol.GetFileResponse(response);

            var fileReceiverSocket = new FileReceiverSocket(
                tcpServerEndpoint.Address, 
                port, 
                _downloadDirectory + "/" + selectedFile.FileName, 
                selectedFile.FileSize);

            while(fileReceiverSocket.BytesReceived < fileReceiverSocket.FileSize)
            {
                Console.WriteLine($"Received {fileReceiverSocket.BytesReceived} bytes out of {fileReceiverSocket.FileSize} bytes.");
            }
            Console.WriteLine($"Received {fileReceiverSocket.BytesReceived} bytes out of {fileReceiverSocket.FileSize} bytes.");
        }

        public void ShareFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            FileManager.Instance.AddFile(fileInfo);
        }

        public void StopShareFile(string fileName,long fileSize,string fileExtension)
        {
            SharingFile sharingFile = new SharingFile();
            sharingFile.FileName = fileName;
            sharingFile.FileExtension = fileExtension;
            sharingFile.FileSize = fileSize;

            FileManager.Instance.RemoveFile(sharingFile);
        }

        public void GetSharingFilesOfCurrentDevice()
        {
            FileManager.Instance.SharingFiles.ForEach(file => Console.WriteLine(file));
        }


    }

}