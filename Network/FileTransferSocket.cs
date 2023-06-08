using System.Net;
using System.Net.Sockets;

namespace NetShare_Core.Network
{
    public class FileSenderSocket
    {   
        private static int BUFFER_SIZE = 1024;
        private Socket _socket;
        private string _filePath;
        private int _port;

        public int Port
        {
            get
            {
                return _port;
            }
        }

        public FileSenderSocket(string absoluteFilePath)
        {
            
        
           
            if(!System.IO.File.Exists(absoluteFilePath))
                throw new System.IO.FileNotFoundException("Requested file does not exist.");

            _filePath = absoluteFilePath;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Any, 0));
            _socket.Listen(1);
            _port = ((IPEndPoint)_socket.LocalEndPoint).Port;

            Task.Factory.StartNew(() =>
            {
                TransferFile();
            });
        }

        private void TransferFile()
        {   

            Socket client = _socket.Accept();

            byte[] buffer = new byte[BUFFER_SIZE];

            int readBytes = 0;

            using (FileStream _fileStream = new FileStream(_filePath, FileMode.Open,FileAccess.Read))
            {
                while ((readBytes = _fileStream.Read(buffer, 0, BUFFER_SIZE)) > 0)
                {
                    client.Send(buffer, 0, readBytes, SocketFlags.None);
                }
            }
            _socket.Close();
            client.Close();
        }

    }
}