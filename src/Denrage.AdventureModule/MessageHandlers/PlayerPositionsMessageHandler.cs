using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.MessageHandlers
{
    public class PlayerPositionsMessageHandler : MessageHandler<PlayerPositionsMessage>
    {
        private readonly Func<LoginService> loginService;
        private readonly Func<PlayerMumbleService> playerMumbleService;

        public PlayerPositionsMessageHandler(Func<LoginService> loginService, Func<PlayerMumbleService> playerMumbleService)
        {
            this.loginService = loginService;
            this.playerMumbleService = playerMumbleService;
        }

        protected override async Task Handle(Guid clientId, PlayerPositionsMessage message, CancellationToken ct)
        {
            foreach (var item in message.Positions)
            {
                if (item.Key == this.loginService().Name)
                {
                    continue;
                }

                _ = this.playerMumbleService().OtherPlayerPositions.AddOrUpdate(item.Key, new Microsoft.Xna.Framework.Vector3(item.Value.X, item.Value.Y, item.Value.Z), (name, oldValue) => new Microsoft.Xna.Framework.Vector3(item.Value.X, item.Value.Y, item.Value.Z));
            }

            await Task.CompletedTask;
        }
    }
}
