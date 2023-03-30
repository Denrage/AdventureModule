using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;

namespace Denrage.AdventureModule.Server.MessageHandlers;

public class GetStatesMessageHandler : MessageHandler<GetStatesMessage>
{
    private readonly Func<SynchronizationService> getSynchronizationService;

    public GetStatesMessageHandler(Func<SynchronizationService> getSynchronizationService)
    {
        this.getSynchronizationService = getSynchronizationService;
    }

    protected override async Task Handle(Guid clientId, GetStatesMessage message, CancellationToken ct)
    {
        await this.getSynchronizationService().GetStates(clientId, ct);
    }
}
