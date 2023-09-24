using Denrage.AdventureModule.Libs.Messages.Handler;

namespace Denrage.AdventureModule.Server.Services;

public interface ITcpService
{
    Task Initialize();
    void RegisterMessage(IMessageHandler handler);
    Task SendMessage<T>(Guid clientId, T message, CancellationToken ct);
    Task SendToGroup<T>(Guid clientId, T message, CancellationToken ct);
    Task SendToGroup<T>(Group group, T message, CancellationToken ct);
}