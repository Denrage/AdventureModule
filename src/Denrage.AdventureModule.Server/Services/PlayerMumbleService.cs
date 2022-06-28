using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;

namespace Denrage.AdventureModule.Server.Services;

public class PlayerMumbleService
{
    private System.Collections.Concurrent.ConcurrentDictionary<string, PlayerPosition> playerPositions = new System.Collections.Concurrent.ConcurrentDictionary<string, PlayerPosition>();
    private readonly TcpService tcpService;
    private readonly UserManagementService userManagementService;

    public PlayerMumbleService(TcpService tcpService, UserManagementService userManagementService)
    {
        this.tcpService = tcpService;
        this.userManagementService = userManagementService;
        _ = Task.Run(async () => this.Run());
    }

    public void UpdatePosition(string name, PlayerPosition position)
    {
        _ = this.playerPositions.AddOrUpdate(name, position, (id, oldPosition) => position);
    }

    public async Task Run()
    {
        while (true)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            foreach (var group in this.userManagementService.Groups)
            {
                var groupPositions = new Dictionary<string, PlayerPosition>();

                foreach (var user in group.Users)
                {
                    if (this.playerPositions.TryGetValue(user.Name, out var position))
                    {
                        groupPositions.Add(user.Name, position);
                    }
                }

                var positionMessage = new PlayerPositionsMessage()
                {
                    Positions = groupPositions,
                };

                await this.tcpService.SendToGroup(group, positionMessage, default);
            }
        }
    }
}
