using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;

namespace Denrage.AdventureModule.Server.MessageHandlers;

public class AddDrawObjectMessageHandler<TDrawObject> : MessageHandler<AddDrawObjectMessage<TDrawObject>>
    where TDrawObject : DrawObject
{
    private readonly IDrawObjectService drawObjectService;

    public AddDrawObjectMessageHandler(IDrawObjectService drawObjectService)
    {
        this.drawObjectService = drawObjectService;
    }

    protected override async Task Handle(Guid clientId, AddDrawObjectMessage<TDrawObject> message, CancellationToken ct)
    {
        await this.drawObjectService.Add(message.DrawObjects, clientId, ct);
    }
}
