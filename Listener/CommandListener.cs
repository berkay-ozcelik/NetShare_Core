using NetShare_Core.Entity;
using NLog;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;


namespace NetShare_Core.Listener
{

    public class CommandListener
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private string _endPoint;
        private Encoding _encoding;


        public CommandListener(string endPoint)
        {
            _endPoint = endPoint;
            _encoding = Encoding.UTF8;
        }

        public void Start()
        {

            using (Socket pipeServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                pipeServer.Bind(IPEndPoint.Parse(_endPoint));

                logger.Info("Pipe server is waiting for connection @" + _endPoint);


                pipeServer.Listen(1);

                Socket pipeClient = pipeServer.Accept();

                logger.Info("Client connected");

                try
                {
                    while (true)
                    {

                        string rawRequest = ReadAll(pipeClient, _encoding);

                        logger.Info("Received request from client: " + rawRequest);

                        Command command = CommandContext.GetCommand(rawRequest);

                        CommandResult commandResult = command.Execute();

                        string response = JsonSerializer.Serialize(commandResult);

                        WriteAll(pipeClient, _encoding, response);

                        logger.Info("Sent response to client: " + response);
                    }
                }
                catch (IOException)
                {

                    logger.Info("Client disconnected.");
                }
            }
        }


        private static string ReadAll(Socket pipeClient, Encoding encoding)
        {

            byte[] buffer = new byte[4];

            pipeClient.Receive(buffer);

            int bufferSize = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[bufferSize];

            pipeClient.Receive(buffer);

            return encoding.GetString(buffer);

        }

        private static void WriteAll(Socket pipeClient, Encoding encoding, string data)
        {
            byte[] buffer = encoding.GetBytes(data);
            byte[] bufferSize = BitConverter.GetBytes(buffer.Length);

            byte[] message = new byte[bufferSize.Length + buffer.Length];

            bufferSize.CopyTo(message, 0);

            buffer.CopyTo(message, bufferSize.Length);

            pipeClient.Send(message);

        }
    }
}
