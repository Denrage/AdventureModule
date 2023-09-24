using Denrage.AdventureModule.Libs.Messages.Data;

namespace Denrage.AdventureModule.Server.Services;

public interface ISynchronizationService
{
    Task GetStates(Guid clientId, CancellationToken ct);
    Task StateChanged(IState state, Guid clientId, CancellationToken ct);
}