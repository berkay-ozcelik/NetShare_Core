using System.Net.Sockets;
using System.Text;

namespace NetShare_Core.Network
{
    public class NetShareSocket
    {   
        public static readonly int RECEIVE_TIMEOUT = 500;
        public static readonly int SEND_TIMEOUT = 500;

        private Socket _socket;
        private Encoding _encoding;
        public NetShareSocket(Socket socket)
        {
            _socket = socket;
            _encoding = Encoding.UTF8;
            _socket.ReceiveTimeout = RECEIVE_TIMEOUT;
        }

        public void Send(string data)
        {
            byte[] byteData = _encoding.GetBytes(data);
            int dataLength = _encoding.GetBytes(data).Length;

            byte[] byteDataLength = BitConverter.GetBytes(dataLength);
            
            _socket.Send(byteDataLength);
            _socket.Send(byteData);

        }

        public string Receive()
        {
            byte[] byteDataLength = new byte[4];
            _socket.Receive(byteDataLength);
            int dataLength = BitConverter.ToInt32(byteDataLength, 0);

            byte[] byteData = new byte[dataLength];
            _socket.Receive(byteData);
            string data = _encoding.GetString(byteData);

            return data;
        }

        public void Terminate()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

    }

}
