using System.Net;
using System.Net.Sockets;
using NetShare_Core.Protocol;
using NLog;
namespace NetShare_Core.Network
{
    public class Acceptor
    {

        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public static readonly int[] UDP_LISTEN_ORDER = { 545, 546, 547 };
        private Socket _TCPServer;
        private Socket _UDPServer;
        private bool _isTCPRunning;
        private bool _isUDPRunning;

        public bool IsTCPRunning
        {
            get
            {
                return _isTCPRunning;
            }
        }
        public bool IsUDPRunning
        {
            get
            {
                return _isUDPRunning;
            }
        }

        public int TCPPort
        {
            get
            {
                return ((IPEndPoint)_TCPServer.LocalEndPoint).Port;
            }
        }

        public int UDPPort
        {
            get
            {
                return ((IPEndPoint)_UDPServer.LocalEndPoint).Port;
            }
        }
        public Acceptor()
        {
            InitTCPServer();

            if (!_isTCPRunning)
                return;

            Task.Factory.StartNew(() =>
            {
                BeginTCPAccept();
            });

            InitUDPServer();

            if (!_isUDPRunning)
                return;

            Task.Factory.StartNew(() =>
            {
                BeginUDPAccept();
            });
        }

        private void InitTCPServer()
        {
            try
            {
                _TCPServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _TCPServer.Bind(new IPEndPoint(IPAddress.Any, 0));
                _TCPServer.Listen(10);
            }
            catch
            {
                logger.Error("TCP Server failed to bind to any port");
                _isTCPRunning = false;
                return;
            }
            logger.Info("TCP Server is listening on port {0}", TCPPort);
            _isTCPRunning = true;
        }

        private void InitUDPServer()
        {
            if (!_isTCPRunning)
            {
                logger.Error("TCP Server is not running, UDP Server cannot be started");
                return;
            }

            _UDPServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            foreach (int port in UDP_LISTEN_ORDER)
            {
                try
                {
                    logger.Info("Attempting to bind to port {0}", port);
                    _UDPServer.Bind(new IPEndPoint(IPAddress.Any, port));
                    logger.Info("UDP Server is listening on port {0}", port);
                    _isUDPRunning = true;
                    break;
                }
                catch
                {
                    logger.Warn("UDP Server failed to bind to port {0}", port);
                }
            }

            if (_isUDPRunning == false)
            {
                logger.Error("UDP Server failed to bind to any port");
                return;
            }
        }

        private void BeginUDPAccept()
        {

            byte[] dummyBuffer = new byte[1024];
            byte[] tcpServerPortByteValue = BitConverter.GetBytes(TCPPort);
            while (true)
            {
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                _UDPServer.ReceiveFrom(dummyBuffer, ref remoteEP);
                _UDPServer.SendTo(tcpServerPortByteValue, remoteEP);
                logger.Info("Sent discover response to {0}", remoteEP);
            }
        }

        private void BeginTCPAccept()
        {
            while (true)
            {
                Socket clientSocket = _TCPServer.Accept();
                logger.Info("Accepted connection from {0}", clientSocket.RemoteEndPoint);
                NetShareSocket netShareSocket = new NetShareSocket(clientSocket);
                Task.Factory.StartNew(() =>
                {
                    string request = netShareSocket.Receive();
                    string response = NetShareProtocol.GenerateResponse(request);
                    netShareSocket.Send(response);
                    logger.Info("Sent response to {0}", clientSocket.RemoteEndPoint);
                    netShareSocket.Terminate();
                });
            }
        }

    }
}