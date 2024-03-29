﻿using Denrage.AdventureModule.Libs.Messages;
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
            SynchronizationService synchronizationService = null;
            PlayerMumbleService playerMumbleService = null;
            var userManagementService = new UserManagementService();
            service = new TcpService(() => drawObjectService, () => synchronizationService, userManagementService, () => playerMumbleService);
            drawObjectService = new DrawObjectService(service, userManagementService);
            drawObjectService.Register<Line, AddDrawObjectMessage<Line>, RemoveDrawObjectMessage<Line>, UpdateDrawObjectMessage<Line>>(
                lines => new AddDrawObjectMessage<Line>() { DrawObjects = lines.ToArray() }, 
                ids => new RemoveDrawObjectMessage<Line>() { Ids = ids.ToArray() }, 
                lines => new UpdateDrawObjectMessage<Line>() { DrawObjects = lines.ToArray() },
                null);
            
            drawObjectService.Register<MapMarker, AddDrawObjectMessage<MapMarker>, RemoveDrawObjectMessage<MapMarker>, UpdateDrawObjectMessage<MapMarker>>(
                markers => new AddDrawObjectMessage<MapMarker>() { DrawObjects = markers.ToArray() }, 
                ids => new RemoveDrawObjectMessage<MapMarker>() { Ids = ids.ToArray() }, 
                markers => new UpdateDrawObjectMessage<MapMarker>() { DrawObjects = markers.ToArray() },
                null);
            
            drawObjectService.Register<Image, AddDrawObjectMessage<Image>, RemoveDrawObjectMessage<Image>, UpdateDrawObjectMessage<Image>>(
                images => new AddDrawObjectMessage<Image>() { DrawObjects = images.ToArray() }, 
                ids => new RemoveDrawObjectMessage<Image>() { Ids = ids.ToArray() }, 
                images => new UpdateDrawObjectMessage<Image>() { DrawObjects = images.ToArray() },
                (oldObject, newObject) =>
                {
                    if (newObject.Location != null)
                    {
                        oldObject.Location = newObject.Location;
                    }

                    if (newObject.Data != null && newObject.Data.Length > 0)
                    {
                        oldObject.Data = newObject.Data;
                    }

                    if (newObject.Opacity.HasValue)
                    {
                        oldObject.Opacity = newObject.Opacity;
                    }

                    if (newObject.Rotation.HasValue)
                    {
                        oldObject.Rotation = newObject.Rotation;
                    }

                    if (newObject.Size != null)
                    {
                        oldObject.Size = newObject.Size;
                    }
                });

            playerMumbleService = new PlayerMumbleService(service, userManagementService);
            synchronizationService = new SynchronizationService(service, userManagementService);
            service.Initialize().Wait();

            Console.WriteLine("Server started");
            Console.ReadLine();
            
        }
    }
}