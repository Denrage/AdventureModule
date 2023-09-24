using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;

namespace Denrage.AdventureModule.Server.MessageHandlers;

public class RemoveDrawObjectMessageHandler<TDrawObject> : MessageHandler<RemoveDrawObjectMessage<TDrawObject>>
    where TDrawObject : DrawObject
{
    private readonly IDrawObjectService drawObjectService;

    public RemoveDrawObjectMessageHandler(IDrawObjectService drawObjectService)
    {
        this.drawObjectService = drawObjectService;
    }

    protected override async Task Handle(Guid clientId, RemoveDrawObjectMessage<TDrawObject> message, CancellationToken ct)
    {
        await this.drawObjectService.Remove<TDrawObject>(message.Ids, clientId, ct);
    }
}
