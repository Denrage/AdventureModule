using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;

namespace Denrage.AdventureModule.Server.MessageHandlers;

public class StateChangedMessageHandler<TState> : MessageHandler<StateChangedMessage<TState>>
    where TState : IState
{
    private readonly Func<SynchronizationService> getSynchronizationService;

    public StateChangedMessageHandler(Func<SynchronizationService> getSynchronizationService)
    {
        this.getSynchronizationService = getSynchronizationService;
    }

    protected override async Task Handle(Guid clientId, StateChangedMessage<TState> message, CancellationToken ct)
    {
        await this.getSynchronizationService().StateChanged(message.State, clientId, ct);
    }
}
