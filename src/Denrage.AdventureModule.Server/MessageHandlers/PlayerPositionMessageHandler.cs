using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Server.MessageHandlers;
public class PlayerPositionMessageHandler : Libs.Messages.Handler.MessageHandler<Libs.Messages.PlayerPositionMessage>
{
    private readonly Func<PlayerMumbleService> playerMumbleService;
    private readonly UserManagementService userManagementService;

    public PlayerPositionMessageHandler(Func<PlayerMumbleService> playerMumbleService, UserManagementService userManagementService)
    {
        this.playerMumbleService = playerMumbleService;
        this.userManagementService = userManagementService;
    }

    protected override Task Handle(Guid clientId, PlayerPositionMessage message, CancellationToken ct)
    {
        this.playerMumbleService().UpdatePosition(this.userManagementService.GetUserFromConnectionId(clientId).Name, message.Position);
        return Task.CompletedTask;
    }
}
