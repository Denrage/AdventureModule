using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;

namespace Denrage.AdventureModule.Server.Services;

public class PlayerMumbleService : IPlayerMumbleService
{
    private System.Collections.Concurrent.ConcurrentDictionary<string, MumbleInformation> playerInformation = new System.Collections.Concurrent.ConcurrentDictionary<string, MumbleInformation>();
    private readonly ITcpService tcpService;
    private readonly IUserManagementService userManagementService;

    public PlayerMumbleService(ITcpService tcpService, IUserManagementService userManagementService)
    {
        this.tcpService = tcpService;
        this.userManagementService = userManagementService;
        _ = Task.Run(async () => this.Run());
    }

    public void UpdateInformation(string name, MumbleInformation information)
    {
        _ = this.playerInformation.AddOrUpdate(name, information, (id, oldPosition) => information);
    }

    public async Task Run()
    {
        while (true)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            foreach (var group in this.userManagementService.Groups)
            {
                var groupInformation = new Dictionary<string, MumbleInformation>();

                foreach (var user in group.Users)
                {
                    if (this.playerInformation.TryGetValue(user.Name, out var information))
                    {
                        groupInformation.Add(user.Name, information);
                    }
                }

                var positionMessage = new PlayersMumbleMessage()
                {
                    Information = groupInformation,
                };

                await this.tcpService.SendToGroup(group, positionMessage, default);
            }
        }
    }
}
