using Autofac;
using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server;
using Denrage.AdventureModule.Server.MessageHandlers;
using Denrage.AdventureModule.Server.Services;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var adventureServer = new AdventureServer();
            adventureServer.Start();
            Console.WriteLine("Server started");
            Console.ReadLine();
        }
    }
}