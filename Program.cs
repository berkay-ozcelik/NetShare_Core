using NetShare_Core.Listener;
namespace NetShare
{

   

    class Program
    {
        static void Main()
        {
            // The name of the Named Pipe to create (same as the client)
            string endPoint = "127.0.0.1:4791";

            var commandListener = new CommandListener(endPoint);

            commandListener.Start();
        }
    }
}
