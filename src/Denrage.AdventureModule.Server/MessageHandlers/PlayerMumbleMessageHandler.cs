using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Server.MessageHandlers;
public class PlayerMumbleMessageHandler : MessageHandler<PlayerMumbleMessage>
{
    private readonly IPlayerMumbleService playerMumbleService;
    private readonly IUserManagementService userManagementService;

    public PlayerMumbleMessageHandler(IPlayerMumbleService playerMumbleService, IUserManagementService userManagementService)
    {
        this.playerMumbleService = playerMumbleService;
        this.userManagementService = userManagementService;
    }

    protected override Task Handle(Guid clientId, PlayerMumbleMessage message, CancellationToken ct)
    {
        if (message == null)
        {
            return Task.CompletedTask;
        }
     
        var user = this.userManagementService.GetUserFromConnectionId(clientId);
        if (user != null)
        {
            this.playerMumbleService.UpdateInformation(user.Name, message.Information);
        }
        return Task.CompletedTask;
    }
}
