using Blish_HUD;
using Denrage.AdventureModule.Libs.Messages;
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
        private Vector3 lastPosition = new Vector3();

        public ConcurrentDictionary<string, (Vector2 MapPosition, Vector3 Position)> OtherPlayerPositions { get; } = new ConcurrentDictionary<string, (Vector2 MapPosition, Vector3 Position)>();

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
                var currentPosition = GameService.Gw2Mumble.PlayerCharacter.Position;
                if (currentPosition != lastPosition)
                {
                    var mapPosition = GameService.Gw2Mumble.UI.MapPosition;
                    await this.tcpService.Send(new PlayerPositionMessage()
                    {
                        Position = new Libs.Messages.Data.PlayerPosition()
                        {
                            Position = new Libs.Messages.Data.Vector3()
                            {
                                X = currentPosition.X,
                                Y = currentPosition.Y,
                                Z = currentPosition.Z,
                            },
                            MapPosition = new Libs.Messages.Data.Vector2()
                            {
                                X = (float)mapPosition.X,
                                Y = (float)mapPosition.Y,
                            },
                        },
                    }, ct);
                }

                this.lastPosition = currentPosition;
            }
        }
    }
}
