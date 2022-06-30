using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;

namespace Denrage.AdventureModule.Server.MessageHandlers;

public class RemoveDrawObjectMessageHandler<TDrawObject> : MessageHandler<RemoveDrawObjectMessage<TDrawObject>>
    where TDrawObject : DrawObject
{
    private readonly Func<DrawObjectService> getDrawObjectService;

    public RemoveDrawObjectMessageHandler(Func<DrawObjectService> getDrawObjectService)
    {
        this.getDrawObjectService = getDrawObjectService;
    }

    protected override async Task Handle(Guid clientId, RemoveDrawObjectMessage<TDrawObject> message, CancellationToken ct)
    {
        await this.getDrawObjectService().Remove<TDrawObject>(message.Ids, clientId, ct);
    }
}
