using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Libs.Messages.Handler;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Adventure
{
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
            await this.getSynchronizationService().StateChanged(message.State, ct);
        }
    }
}
