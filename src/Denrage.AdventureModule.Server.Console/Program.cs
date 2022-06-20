using Denrage.AdventureModule.Server;
using Denrage.AdventureModule.Server.Services;
using System;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpService service = null;
            WhiteboardService whiteboardService = null;
            service = new TcpService(() => whiteboardService);
            whiteboardService = new WhiteboardService(service, new UserManagementService());
            service.Initialize().Wait();

            Console.WriteLine("Server started");
            Console.ReadLine();
            
        }
    }
}