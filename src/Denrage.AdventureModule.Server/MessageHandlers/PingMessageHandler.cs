using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;

namespace Denrage.AdventureModule.Server.MessageHandlers;

public class PingMessageHandler : MessageHandler<PingMessage>
{
    private readonly ITcpService tcpService;

    public PingMessageHandler(ITcpService tcpService)
    {
        this.tcpService = tcpService;
    }

    protected override async Task Handle(Guid clientId, PingMessage message, CancellationToken ct)
    {
        await this.tcpService.SendMessage(clientId, new PingResponseMessage() { Id = message.Id }, ct);
    }
}
