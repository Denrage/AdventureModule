using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Adventure
{
    public class SynchronizationService
    {
        private readonly ConcurrentDictionary<Type, Action<IState>> stateHandlers = new ConcurrentDictionary<Type, Action<IState>>();
        private readonly TcpService tcpService;

        public SynchronizationService(TcpService tcpService)
        {
            this.tcpService = tcpService;
        }

        public void Register(Type stateType, Action<IState> onStateReceived)
        {
            this.stateHandlers.TryAdd(stateType, onStateReceived);
        }

        public Task StateChanged(IState state, CancellationToken ct)
        {
            if (this.stateHandlers.TryGetValue(state.GetType(), out var handler))
            {
                handler(state);
            }

            return Task.CompletedTask;
        }

        public async Task SendNewState<T>(T state, CancellationToken ct)
            where T : IState
        {
            await this.tcpService.Send(new StateChangedMessage<T>() { State = state }, ct);
        }
    }
}
