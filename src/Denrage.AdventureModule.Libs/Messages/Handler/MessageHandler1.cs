using Denrage.AdventureModule.Libs.Messages.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Libs.Messages.Handler
{
    public abstract class MessageHandler<T> : MessageHandler
        where T : Message
    {
        public override Type MessageType { get; } = typeof(T);

        public override async Task Handle(Guid clientId, object message, CancellationToken ct)
            => await this.Handle(clientId, (T)message, ct);

        protected abstract Task Handle(Guid clientId, T message, CancellationToken ct);
    }
}