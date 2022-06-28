using Blish_HUD;
using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Services
{
    public class PlayerMumbleService
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly TcpService tcpService;
        private MumbleInformation lastInformation;

        public ConcurrentDictionary<string, MumbleInformation> OtherPlayerInformation { get; } = new ConcurrentDictionary<string, MumbleInformation>();

        public PlayerMumbleService(TcpService tcpService)
        {
            this.tcpService = tcpService;
            _ = Task.Run(async () => await this.Run(this.cancellationTokenSource.Token));
        }

        private async Task Run(CancellationToken ct)
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromMilliseconds(50), ct);

                if (!this.lastInformation.Equal(GameService.Gw2Mumble))
                {
                    var information = GameService.Gw2Mumble.ToInformation();
                    this.lastInformation = information;
                    await this.tcpService.Send(new PlayerMumbleMessage()
                    {
                        Information = information,
                    }, ct);
                }
            }
        }
    }

    public static class MumbleExtensions
    {
        public static bool Equal(this Libs.Messages.Data.Vector3 messageVector, Microsoft.Xna.Framework.Vector3 xnaVector)
            => messageVector.X == xnaVector.X && messageVector.Y == xnaVector.Y && messageVector.Z == xnaVector.Z;

        public static bool Equal(this Libs.Messages.Data.Vector2 messageVector, Microsoft.Xna.Framework.Vector2 xnaVector)
            => messageVector.X == xnaVector.X && messageVector.Y == xnaVector.Y;

        public static bool Equal(this Libs.Messages.Data.Vector2 messageVector, Coordinates2 coordinates)
            => messageVector.X == coordinates.X && messageVector.Y == coordinates.Y;

        public static bool Equal(this Libs.Messages.Data.MumbleInformation mumbleInformation, Gw2MumbleService mumbleService)
            => mumbleInformation != null && mumbleInformation.CameraPosition.Equal(mumbleService.PlayerCamera.Position) &&
                mumbleInformation.MapPosition.Equal(mumbleService.PlayerCharacter.Position) &&
                mumbleInformation.ContinentPosition.Equal(mumbleService.UI.MapPosition) &&
                mumbleInformation.MapId == mumbleService.CurrentMap.Id &&
                mumbleInformation.CameraDirection.Equal(mumbleService.PlayerCamera.Forward) &&
                mumbleInformation.CharacterName == mumbleService.PlayerCharacter.Name;

        public static Libs.Messages.Data.Vector2 ToMessageVector(this Microsoft.Xna.Framework.Vector2 vector)
            => new Libs.Messages.Data.Vector2()
            {
                X = vector.X,
                Y = vector.Y,
            };

        public static Libs.Messages.Data.Vector2 ToMessageVector(this Coordinates2 coordinates)
            => new Libs.Messages.Data.Vector2()
            {
                X = (float)coordinates.X,
                Y = (float)coordinates.Y,
            };

        public static Libs.Messages.Data.Vector3 ToMessageVector(this Microsoft.Xna.Framework.Vector3 vector)
            => new Libs.Messages.Data.Vector3()
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z,
            };

        public static Microsoft.Xna.Framework.Vector2 ToVector(this Libs.Messages.Data.Vector2 vector)
            => new Microsoft.Xna.Framework.Vector2()
            {
                X = vector.X,
                Y = vector.Y,
            };

        public static Microsoft.Xna.Framework.Vector3 ToVector(this Libs.Messages.Data.Vector3 vector)
            => new Microsoft.Xna.Framework.Vector3()
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z,
            };

        public static MumbleInformation ToInformation(this Gw2MumbleService service)
            => new MumbleInformation
            {
                CameraDirection = service.PlayerCamera.Forward.ToMessageVector(),
                CharacterName = service.PlayerCharacter.Name,
                CameraPosition = service.PlayerCamera.Position.ToMessageVector(),
                ContinentPosition = service.UI.MapPosition.ToMessageVector(),
                MapId = service.CurrentMap.Id,
                MapPosition = service.PlayerCharacter.Position.ToMessageVector(),
            };
    }
}
