using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Server.MessageHandlers;
internal class LoginMessageHandler : MessageHandler<LoginMessage>
{
    private readonly UserManagementService userManagementService;

    public LoginMessageHandler(UserManagementService userManagementService)
    {
        this.userManagementService = userManagementService;
    }

    protected override async Task Handle(Guid clientId, LoginMessage message, CancellationToken ct)
    {
        var user = this.userManagementService.GetUserFromConnectionId(clientId);
        if (user != null)
        {
            this.userManagementService.UpdateUserId(clientId, message.Name);
        }
        else
        {
            this.userManagementService.AddUser(clientId, message.Name);
        }

        await Task.CompletedTask;
    }
}
