using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;

namespace Denrage.AdventureModule.Server.MessageHandlers;

public class WhiteboardAddLineMessageHandler : MessageHandler<WhiteboardAddLineMessage>
{
    private readonly Func<WhiteboardService> getWhiteboardService;

    public WhiteboardAddLineMessageHandler(Func<WhiteboardService> getWhiteboardService)
    {
        this.getWhiteboardService = getWhiteboardService;
    }

    protected override async Task Handle(Guid clientId, WhiteboardAddLineMessage message, CancellationToken ct)
    {
        await this.getWhiteboardService().AddLines(clientId, message.Lines, ct);
    }
}
