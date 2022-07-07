using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;

namespace Denrage.AdventureModule.Server.MessageHandlers;

public class UpdateDrawObjectMessageHandler<TDrawObject> : MessageHandler<UpdateDrawObjectMessage<TDrawObject>>
    where TDrawObject : DrawObject
{
    private readonly Func<DrawObjectService> getDrawObjectService;

    public UpdateDrawObjectMessageHandler(Func<DrawObjectService> getDrawObjectService)
    {
        this.getDrawObjectService = getDrawObjectService;
    }

    protected override async Task Handle(Guid clientId, UpdateDrawObjectMessage<TDrawObject> message, CancellationToken ct)
    {
        await this.getDrawObjectService().Update(message.DrawObjects, clientId, ct);
    }
}
