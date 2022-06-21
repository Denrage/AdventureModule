using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;

namespace Denrage.AdventureModule.Server.MessageHandlers;

public class WhiteboardRemoveLineMessageHandler : MessageHandler<WhiteboardRemoveLineMessage>
{
    private readonly Func<WhiteboardService> getWhiteboardService;

    public WhiteboardRemoveLineMessageHandler(Func<WhiteboardService> getWhiteboardService)
    {
        this.getWhiteboardService = getWhiteboardService;
    }

    protected override async Task Handle(Guid clientId, WhiteboardRemoveLineMessage message, CancellationToken ct)
    {
        await this.getWhiteboardService().RemoveLines(clientId, message.Ids, ct);
    }
}
