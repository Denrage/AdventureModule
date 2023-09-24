using System;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Libs.Messages.Handler
{
    public interface IMessageHandler
    {
        Type MessageType { get; }

        Task Handle(Guid clientId, object message, CancellationToken ct);
    }
}