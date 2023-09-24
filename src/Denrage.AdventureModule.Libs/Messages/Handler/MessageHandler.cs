using System;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Libs.Messages.Handler
{
    public abstract class MessageHandler : IMessageHandler
    {
        public abstract Type MessageType { get; }

        public abstract Task Handle(Guid clientId, object message, CancellationToken ct);
    }
}