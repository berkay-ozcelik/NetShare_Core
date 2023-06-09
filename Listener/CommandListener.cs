using NetShare_Core.Entity;
using NLog;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;


namespace NetShare_Core.Listener
{

    public class CommandListener
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private string _pipeName;
        private Encoding _encoding;


        public CommandListener(string pipeName)
        {
            _pipeName = pipeName;
            _encoding = Encoding.UTF8;
        }

        public void Start()
        {
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(_pipeName))
            {
                logger.Info("Named Pipe server is waiting for connection");

                
                pipeServer.WaitForConnection();

                logger.Info("Client connected");

                try
                {
                    while (true)
                    {

                        string rawRequest = ReadAll(pipeServer, _encoding);

                        Command command = CommandContext.GetCommand(rawRequest);

                        CommandResult commandResult = command.Execute();

                        string response = JsonSerializer.Serialize(commandResult);

                        WriteAll(pipeServer, _encoding, response);

                        logger.Info("Sent response to client: " + response);
                    }
                }
                catch (IOException)
                {
                    
                    logger.Info("Client disconnected.");
                }
            }
        }


        private static string ReadAll(NamedPipeServerStream pipeServer, Encoding encoding)
        {

            byte[] buffer = new byte[4];

            pipeServer.Read(buffer, 0, buffer.Length);

            int bufferSize = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[bufferSize];

            pipeServer.Read(buffer, 0, buffer.Length);

            return encoding.GetString(buffer);

        }

        private static void WriteAll(NamedPipeServerStream pipeServer, Encoding encoding, string data)
        {
            byte[] buffer = encoding.GetBytes(data);
            byte[] bufferSize = BitConverter.GetBytes(buffer.Length);

            byte[] message = new byte[bufferSize.Length + buffer.Length];

            bufferSize.CopyTo(message, 0);

            buffer.CopyTo(message, bufferSize.Length);


            pipeServer.Write(message, 0, message.Length);
            
            pipeServer.Flush();
        }
    }
}
