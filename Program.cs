using NetShare_Core.Listener;
namespace NetShare
{

   

    class Program
    {
        static void Main()
        {
            // The name of the Named Pipe to create (same as the client)
            string _pipeName = "NetShare";

            var commandListener = new CommandListener(_pipeName);

            commandListener.Start();
        }
    }
}
