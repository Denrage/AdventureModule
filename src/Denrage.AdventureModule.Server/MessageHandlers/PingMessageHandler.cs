using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;

namespace Denrage.AdventureModule.Server.MessageHandlers;

public class PingMessageHandler : MessageHandler<PingMessage>
{
    private readonly TcpService tcpService;

    public PingMessageHandler(TcpService tcpService)
    {
        this.tcpService = tcpService;
    }

    protected override async Task Handle(Guid clientId, PingMessage message, CancellationToken ct)
    {
        Console.WriteLine("Got Ping, sending ping back");
        await this.tcpService.SendMessage(clientId, new PingMessage(), ct);
    }
}
