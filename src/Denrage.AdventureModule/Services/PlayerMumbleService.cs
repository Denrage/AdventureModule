using Blish_HUD;
using Denrage.AdventureModule.Helper;
using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
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

        public PlayerMumbleService(TcpService tcpService, LoginService loginService)
        {
            this.tcpService = tcpService;

            loginService.LoggedIn += () => 
                _ = Task.Run(async () => await this.Run(this.cancellationTokenSource.Token));
        }

        private async Task Run(CancellationToken ct)
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromMilliseconds(50), ct);

                if (!GameService.Gw2Mumble.IsAvailable)
                {
                    continue;
                }

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
}
