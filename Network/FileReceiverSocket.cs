using System.Net;
using System.Net.Sockets;

namespace NetShare_Core.Network
{
    public class FileReceiverSocket
    {   
        private static int BUFFER_SIZE = 1024;
        private Socket _socket;
        private string _filePath;
        private long _fileSize;
        private long _bytesReceived;
        private IPEndPoint _endPoint;
        
        public long BytesReceived
        {
            get
            {
                return _bytesReceived;
            }
        }

        public long FileSize
        {
            get
            {
                return _fileSize;
            }
        }
        public FileReceiverSocket(IPAddress adress,int port, string filePath,long fileSize)
        {   
            if(System.IO.File.Exists(filePath))
                throw new System.IO.IOException("File already exists.");

            _endPoint = new IPEndPoint(adress, port);
            _filePath = filePath;
            _fileSize = fileSize;
            _bytesReceived = 0;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(_endPoint);

            Task.Factory.StartNew(() =>
            {
                ReceiveFile();
            });
        }

        private void ReceiveFile()
        {
            byte[] buffer = new byte[BUFFER_SIZE];

            int readBytes = 0;

            using (FileStream fileStream = new FileStream(_filePath, FileMode.Create))
            {
                while ((readBytes = _socket.Receive(buffer)) > 0)
                {
                    _bytesReceived += readBytes;
                    fileStream.Write(buffer, 0, readBytes);
                }
            }
        }

    }
}