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
            var userManagementService = new UserManagementService();
            service = new TcpService(() => whiteboardService, userManagementService);
            whiteboardService = new WhiteboardService(service, userManagementService);
            service.Initialize().Wait();

            Console.WriteLine("Server started");
            Console.ReadLine();
            
        }
    }
}