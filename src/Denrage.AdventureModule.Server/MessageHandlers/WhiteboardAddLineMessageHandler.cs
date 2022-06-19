using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;

namespace Denrage.AdventureModule.Server.MessageHandlers;

public class WhiteboardAddLineMessageHandler : MessageHandler<WhiteboardAddLineMessage>
{
    private readonly WhiteboardService whiteboardService;

    public WhiteboardAddLineMessageHandler(WhiteboardService whiteboardService)
    {
        this.whiteboardService = whiteboardService;
    }

    protected override async Task Handle(Guid clientId, WhiteboardAddLineMessage message, CancellationToken ct)
    {
        await this.whiteboardService.AddLines(clientId, message.Lines, ct);
    }
}
