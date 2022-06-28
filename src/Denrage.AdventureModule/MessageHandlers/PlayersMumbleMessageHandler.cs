using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.MessageHandlers
{
    public class PlayersMumbleMessageHandler : MessageHandler<PlayersMumbleMessage>
    {
        private readonly Func<LoginService> loginService;
        private readonly Func<PlayerMumbleService> playerMumbleService;

        public PlayersMumbleMessageHandler(Func<LoginService> loginService, Func<PlayerMumbleService> playerMumbleService)
        {
            this.loginService = loginService;
            this.playerMumbleService = playerMumbleService;
        }

        protected override async Task Handle(Guid clientId, PlayersMumbleMessage message, CancellationToken ct)
        {
            foreach (var item in message.Information)
            {
                if (item.Key == this.loginService().Name)
                {
                    continue;
                }

                if (item.Value is null)
                {
                    continue;
                }

                _ = this.playerMumbleService().OtherPlayerInformation.AddOrUpdate(item.Key, item.Value, (name, oldValue) => item.Value);
            }

            await Task.CompletedTask;
        }
    }
}
