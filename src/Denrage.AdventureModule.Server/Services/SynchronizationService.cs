using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using System.Collections.Concurrent;

namespace Denrage.AdventureModule.Server.Services;

public class SynchronizationService : ISynchronizationService
{
    private readonly ITcpService tcpService;
    private readonly IUserManagementService userManagementService;
    private readonly ConcurrentDictionary<Guid, IState> currentStates = new ConcurrentDictionary<Guid, IState>();

    public SynchronizationService(ITcpService tcpService, IUserManagementService userManagementService)
    {
        this.tcpService = tcpService;
        this.userManagementService = userManagementService;
    }

    public async Task GetStates(Guid clientId, CancellationToken ct)
    {
        foreach (var item in this.currentStates)
        {
            if (item.Value is DialogState dialogState)
            {
                await this.tcpService.SendMessage(clientId, new StateChangedMessage<DialogState>() { State = dialogState }, default);
            }
            else if (item.Value is AdventureState adventureState)
            {
                await this.tcpService.SendMessage(clientId, new StateChangedMessage<AdventureState>() { State = adventureState }, default);
            }
            else if (item.Value is LuaVariablesState variablesState)
            {
                await this.tcpService.SendMessage(clientId, new StateChangedMessage<LuaVariablesState>() { State = variablesState }, default);
            }
        }
    }

    public async Task StateChanged(IState state, Guid clientId, CancellationToken ct)
    {
        _ = this.currentStates.TryAdd(state.Id, state);
        var group = this.userManagementService.GetGroup(this.userManagementService.GetUserFromConnectionId(clientId));

        if (state is DialogState dialogState)
        {
            await this.tcpService.SendToGroup(group, new StateChangedMessage<DialogState>() { State = dialogState }, ct);
        }

        if (state is LuaVariablesState variableState)
        {
            await this.tcpService.SendToGroup(group, new StateChangedMessage<LuaVariablesState>() { State = variableState }, ct);
        }

        if (state is AdventureState adventureState)
        {
            await this.tcpService.SendToGroup(group, new StateChangedMessage<AdventureState>() { State = adventureState }, ct);
        }
    }
}
