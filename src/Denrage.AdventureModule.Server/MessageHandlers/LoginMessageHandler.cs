using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Server.MessageHandlers;
public class LoginMessageHandler : MessageHandler<LoginMessage>
{
    private readonly IUserManagementService userManagementService;
    private readonly ITcpService tcpService;

    public LoginMessageHandler(IUserManagementService userManagementService, ITcpService tcpService)
    {
        this.userManagementService = userManagementService;
        this.tcpService = tcpService;
    }

    protected override async Task Handle(Guid clientId, LoginMessage message, CancellationToken ct)
    {
        var user = this.userManagementService.GetUserFromConnectionId(clientId);
        if (user != null)
        {
            Console.WriteLine($"User '{user.Name}' changed name to '{message.Name}'");
            this.userManagementService.UpdateUserId(clientId, message.Name);
        }
        else
        {
            if (this.userManagementService.UserExists(message.Name))
            {
                this.userManagementService.UpdateUserId(clientId, message.Name);
                Console.WriteLine($"User '{message.Name}' logged back in");
            }
            else
            {
                this.userManagementService.AddUser(clientId, message.Name);
                Console.WriteLine($"New User '{message.Name}' logged in");
            }
        }

        await this.tcpService.SendMessage(clientId, new LoginResponseMessage() { Id = message.Id, Success = true }, ct);
    }
}
