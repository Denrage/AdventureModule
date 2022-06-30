using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Server.MessageHandlers;
public class PlayerMumbleMessageHandler : Libs.Messages.Handler.MessageHandler<Libs.Messages.PlayerMumbleMessage>
{
    private readonly Func<PlayerMumbleService> playerMumbleService;
    private readonly UserManagementService userManagementService;

    public PlayerMumbleMessageHandler(Func<PlayerMumbleService> playerMumbleService, UserManagementService userManagementService)
    {
        this.playerMumbleService = playerMumbleService;
        this.userManagementService = userManagementService;
    }

    protected override Task Handle(Guid clientId, PlayerMumbleMessage message, CancellationToken ct)
    {
        var mumbleService = this.playerMumbleService();
        if (message == null)
        {
            return Task.CompletedTask;
        }
        var user = this.userManagementService.GetUserFromConnectionId(clientId);
        if (user != null)
        {
            mumbleService.UpdateInformation(user.Name, message.Information);
        }
        return Task.CompletedTask;
    }
}
