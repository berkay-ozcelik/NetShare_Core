using NetShare_Core.Network;
using NetShare_Core.Entity;
using NetShare_Core.Device;
using System.Net;
using NetShare_Core.Protocol;

namespace NetShare
{

    public static class Facade
    {
        
        private static Acceptor _acceptor;
        private static DeviceInfo[] _devices;
        private static int _selectedDeviceIndex;
        private static List<SharingFile> _sharingFilesOfSelectedDevice;
        private static int _selectedFileIndex;
        private static string _downloadDirectory;
        private static Dictionary<int,FileReceiverSocket> _activeDownloads;
        private static int _downloadId;

        
        static Facade()
        {
            _downloadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Downloads");
            _activeDownloads = new Dictionary<int,FileReceiverSocket>();
            _downloadId = 0;
            if (!Directory.Exists(_downloadDirectory))
            {
                Directory.CreateDirectory(_downloadDirectory);
            }
        }

        public static void StartAcceptor()
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

        public static DeviceInfo[] DiscoverDevices()
        {
            _devices = Requestor.Discover();
            _sharingFilesOfSelectedDevice = null;

            return _devices;
        }

        public static void SelectDevice(int index)
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

        public static List<SharingFile> GetSharingFiles()
        {
            var selectedDevice = _devices[_selectedDeviceIndex];

            var tcpServerEndpoint = IPEndPoint.Parse(selectedDevice.EndPoint);

            var response = Requestor.SendRequest(
                        NetShareProtocol.FileListRequest(),
                        tcpServerEndpoint);

            _sharingFilesOfSelectedDevice = NetShareProtocol.FileListResponse(response);

            return _sharingFilesOfSelectedDevice;
        }

        public static void SelectFile(int index)
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

        public static void SetDownloadDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new Exception("Invalid directory");
            }

            _downloadDirectory = directory;
        }


        public static int StartDownload()
        {

            var selectedDevice = _devices[_selectedDeviceIndex];
            var selectedFile = _sharingFilesOfSelectedDevice[_selectedFileIndex];
            
            //TODO: Check if selected file still sharing
            
            var filePath = Path.Combine(_downloadDirectory, selectedFile.FileName);

            if (File.Exists(filePath))
                throw new IOException("File already exists.");

            try
            {
                var tcpServerEndpoint = IPEndPoint.Parse(selectedDevice.EndPoint);

                var fileDownloadRequest = NetShareProtocol.GetFileRequest(selectedFile);

                var response = Requestor.SendRequest(fileDownloadRequest, tcpServerEndpoint);

                var port = NetShareProtocol.GetFileResponse(response);

                FileReceiverSocket activeDownload = new FileReceiverSocket(
                    tcpServerEndpoint.Address,
                    port,
                    filePath,
                    selectedFile.FileSize);

                _activeDownloads.Add(++_downloadId, activeDownload);

                return _downloadId;
               
            }
            catch
            {   
                
                throw new Exception("Error while starting download");
            }

        }

        public static int GetDownloadProgress(int downloadId)
        {
            FileReceiverSocket activeDownload;
            
            if (!_activeDownloads.TryGetValue(downloadId,out activeDownload))
                throw new Exception("No download in progress with given ID");

            if(activeDownload.IsCanceled)
                throw new Exception("Download canceled");

            if (activeDownload.IsFailed)
                throw new Exception("Download failed");

            if (activeDownload.IsCompleted)
                return 100;

            var received = activeDownload.BytesReceived;
            var total = activeDownload.FileSize;
            var progress = (int)(received * 100 / total);

            return progress;
        }
        
        public static void StopDownload(int downloadId)
        {
            FileReceiverSocket activeDownload;

            if (!_activeDownloads.TryGetValue(downloadId, out activeDownload))
                throw new Exception("No download in progress with given ID");

            activeDownload.Stop();
        }

        public static void ShareFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new Exception("File does not exist");
            }

            //Check if file is already sharing
            var file = FileManager.Instance.SharingFiles.Find(f => f.FilePath == filePath);
            if (file != null)
                throw new Exception("File is already sharing");

            FileManager.Instance.AddFile(filePath);
        }

        public static void StopShareFile(int index)
        {
            FileManager.Instance.RemoveFile(index);
        }

        public static List<SharingFile> GetSharingFilesOfCurrentDevice()
        {
            return FileManager.Instance.SharingFiles;
        }


    }

}