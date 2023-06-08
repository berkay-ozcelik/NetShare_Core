using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetShare_Core.Device;
using NetShare_Core.Entity;
using NetShare_Core.Protocol;
using NLog;

namespace NetShare_Core.Network
{
    public static class Requestor
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public static string SendRequest(string request,IPEndPoint endPoint)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endPoint);
            NetShareSocket netShareSocket = new NetShareSocket(socket);
            netShareSocket.Send(request);
            string response = netShareSocket.Receive();
            netShareSocket.Terminate();
            return response;
        }  

        public static DeviceInfo[] Discover()
        {
            List<DeviceInfo> devices = new List<DeviceInfo>();

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.ReceiveTimeout = NetShareSocket.RECEIVE_TIMEOUT;

            socket.EnableBroadcast = true;

            byte[] dummyBuffer = new byte[0];

            foreach (int port in Acceptor.UDP_LISTEN_ORDER)
            {
                socket.SendTo(dummyBuffer, new IPEndPoint(IPAddress.Broadcast, port));
                byte[] portBuffer = new byte[4];
                
                while(true)
                {

                    logger.Info($"UDP searching starting for port {port}");

                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    try
                    {
                        socket.ReceiveFrom(portBuffer, ref remoteEndPoint);
                    }
                    catch
                    {
                        logger.Info($"UDP searching done for port {port}");
                        break;
                    }

                    int tcpPort = BitConverter.ToInt32(portBuffer, 0);

                    IPEndPoint tcpServer = new IPEndPoint(((IPEndPoint)remoteEndPoint).Address, tcpPort);

                    logger.Info($"TCP server found at {tcpServer.ToString()}");

                    string response = Requestor.SendRequest(NetShareProtocol.IdentificationRequest(), tcpServer);
                    var device = NetShareProtocol.IdentificationResponse(response);
                    device.EndPoint = tcpServer.ToString();
                    devices.Add(device);
                }
            }
            return devices.ToArray();
        }         
    }
}