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
                if (_instance == null)
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
            _downloadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Downloads");
            
            if (!Directory.Exists(_downloadDirectory))
            {
                Directory.CreateDirectory(_downloadDirectory);
            }
        }

        public void StartAcceptor()
        {
            _acceptor = new Acceptor();

            if (!_acceptor.IsTCPRunning)
            {
                throw new Exception("TCP Server is not running");
            }

            if (!_acceptor.IsUDPRunning)
            {
                throw new Exception("UDP Server is not running");
            }
        }

        public void DiscoverDevices()
        {
            _devices = Requestor.Discover();
            _sharingFilesOfSelectedDevice = null;

            // Print devices with index
            for (int i = 0; i < _devices.Length; i++)
            {
                Console.WriteLine($"[{i}] {_devices[i]}");
            }
        }

        public void SelectDevice(int index)
        {
            if (_devices == null)
            {
                throw new Exception("Devices not discovered yet");
            }

            if (index < 0 || index >= _devices.Length)
            {
                throw new Exception("Invalid device index");
            }

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
            if (_sharingFilesOfSelectedDevice == null)
            {
                throw new Exception("Files not retrieved yet");
            }

            if (index < 0 || index >= _sharingFilesOfSelectedDevice.Count)
            {
                throw new Exception("Invalid file index");
            }

            _selectedFileIndex = index;
        }

        public void SetDownloadDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new Exception("Invalid directory");
            }

            _downloadDirectory = directory;
        }

        public void DownloadFile()
        {


            var selectedDevice = _devices[_selectedDeviceIndex];
            var selectedFile = _sharingFilesOfSelectedDevice[_selectedFileIndex];
            var filePath = Path.Combine(_downloadDirectory, selectedFile.FileName);

            if (File.Exists(filePath))
                throw new IOException("File already exists.");

            try
            {
                var tcpServerEndpoint = IPEndPoint.Parse(selectedDevice.EndPoint);

                var fileDownloadRequest = NetShareProtocol.GetFileRequest(selectedFile);

                var response = Requestor.SendRequest(fileDownloadRequest, tcpServerEndpoint);

                var port = NetShareProtocol.GetFileResponse(response);

                var fileReceiverSocket = new FileReceiverSocket(
                    tcpServerEndpoint.Address,
                    port,
                    filePath,
                    selectedFile.FileSize);

                while (fileReceiverSocket.BytesReceived < fileReceiverSocket.FileSize)
                {
                    if (fileReceiverSocket.IsFailed)
                    {
                        throw new Exception($"Error while downloading file at byte {fileReceiverSocket.BytesReceived}");
                    }
                    Console.WriteLine($"Received {fileReceiverSocket.BytesReceived} bytes out of {fileReceiverSocket.FileSize} bytes.");
                }
                Console.WriteLine($"Received {fileReceiverSocket.BytesReceived} bytes out of {fileReceiverSocket.FileSize} bytes.");
            }
            catch
            {
                throw new Exception("Error while downloading file");
            }

        }

        public void ShareFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new Exception("File does not exist");
            }

            FileManager.Instance.AddFile(filePath);
        }

        public void StopShareFile(string fileName, long fileSize, string fileExtension)
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