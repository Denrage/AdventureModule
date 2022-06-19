using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.MessageHandlers
{
    public class PingMessageHandler : MessageHandler<PingMessage>
    {
        private readonly TcpService tcpService;

        public PingMessageHandler(TcpService tcpService)
        {
            this.tcpService = tcpService;
        }

        protected override async Task Handle(Guid clientId, PingMessage message, CancellationToken ct)
        {
            Module.Logger.Info("Ping received");
            this.tcpService.HeartbeatReceived();
            await Task.CompletedTask;
        }
    }
}
