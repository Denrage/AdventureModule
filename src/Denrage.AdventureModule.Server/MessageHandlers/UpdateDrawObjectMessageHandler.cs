using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;

namespace Denrage.AdventureModule.Server.MessageHandlers;

public class UpdateDrawObjectMessageHandler<TDrawObject> : MessageHandler<UpdateDrawObjectMessage<TDrawObject>>
    where TDrawObject : DrawObject
{
    private readonly IDrawObjectService drawObjectService;

    public UpdateDrawObjectMessageHandler(IDrawObjectService drawObjectService)
    {
        this.drawObjectService = drawObjectService;
    }

    protected override async Task Handle(Guid clientId, UpdateDrawObjectMessage<TDrawObject> message, CancellationToken ct)
    {
        await this.drawObjectService.Update(message.DrawObjects, clientId, ct);
    }
}
