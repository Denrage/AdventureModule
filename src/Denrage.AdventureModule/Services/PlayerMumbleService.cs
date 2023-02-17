using Blish_HUD;
using Denrage.AdventureModule.Helper;
using Denrage.AdventureModule.Interfaces.Mumble;
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
        private readonly IGw2Mumble gw2Mumble;
        private MumbleInformation lastInformation;

        public ConcurrentDictionary<string, MumbleInformation> OtherPlayerInformation { get; } = new ConcurrentDictionary<string, MumbleInformation>();

        public PlayerMumbleService(TcpService tcpService, LoginService loginService, IGw2Mumble gw2Mumble)
        {
            this.tcpService = tcpService;
            this.gw2Mumble = gw2Mumble;
            loginService.LoggedIn += () => 
                _ = Task.Run(async () => await this.Run(this.cancellationTokenSource.Token));
        }

        private async Task Run(CancellationToken ct)
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromMilliseconds(50), ct);

                if (!this.gw2Mumble.IsAvailable)
                {
                    continue;
                }

                if (!this.lastInformation.Equal(this.gw2Mumble))
                {
                    var information = this.gw2Mumble.ToInformation();
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
