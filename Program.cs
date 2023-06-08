using System.ComponentModel;

namespace NetShare
{
    class Program
    {
        static void Main(string[] args)
        {
            Facade.Instance.StartAcceptor();
            
            REFRESH:
            Facade.Instance.DiscoverDevices();
            
            Console.Write("Select device:");
            int index = int.Parse(Console.ReadLine());

            if (index < 0)
                goto REFRESH;

            Facade.Instance.SelectDevice(index);

            Facade.Instance.GetSharingFiles();
            Console.Write("Select file:");
            index = int.Parse(Console.ReadLine());
            Facade.Instance.SelectFile(index);
            
            //Navigate to download directory
            Console.Write("Enter download directory:");
            string downloadDirectory = Console.ReadLine();
            Facade.Instance.SetDownloadDirectory(downloadDirectory);

            Facade.Instance.DownloadFile();



            Console.ReadKey();
        }
    }
}
