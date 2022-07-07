using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
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
            DrawObjectService drawObjectService = null;
            PlayerMumbleService playerMumbleService = null;
            var userManagementService = new UserManagementService();
            service = new TcpService(() => drawObjectService, userManagementService, () => playerMumbleService);
            drawObjectService = new DrawObjectService(service, userManagementService);
            drawObjectService.Register<Line, AddDrawObjectMessage<Line>, RemoveDrawObjectMessage<Line>>(lines => new AddDrawObjectMessage<Line>() { DrawObjects = lines.ToArray() }, ids => new RemoveDrawObjectMessage<Line>() { Ids = ids.ToArray() });
            drawObjectService.Register<MapMarker, AddDrawObjectMessage<MapMarker>, RemoveDrawObjectMessage<MapMarker>>(markers => new AddDrawObjectMessage<MapMarker>() { DrawObjects = markers.ToArray() }, ids => new RemoveDrawObjectMessage<MapMarker>() { Ids = ids.ToArray() });
            drawObjectService.Register<Image, AddDrawObjectMessage<Image>, RemoveDrawObjectMessage<Image>>(marker => new AddDrawObjectMessage<Image>() { DrawObjects = marker.ToArray() }, ids => new RemoveDrawObjectMessage<Image>() { Ids = ids.ToArray() });
            playerMumbleService = new PlayerMumbleService(service, userManagementService);
            service.Initialize().Wait();

            Console.WriteLine("Server started");
            Console.ReadLine();
            
        }
    }
}