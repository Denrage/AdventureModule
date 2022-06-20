using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using System.Collections.Concurrent;

namespace Denrage.AdventureModule.Server.Services;

public class WhiteboardService
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<Line>> lines = new();

    private TcpService tcpService;
    private readonly UserManagementService userManagementService;

    public WhiteboardService(TcpService tcpService, UserManagementService userManagementService)
    {
        this.tcpService = tcpService;
        this.userManagementService = userManagementService;
        this.userManagementService.LoggedIn += id =>
        {
            var user = this.userManagementService.GetUserFromConnectionId(id);
            _ = this.tcpService.SendMessage(id, new WhiteboardAddLineMessage()
            {
                Lines = this.lines.Where(x => x.Key != user.Name).Select(x => x.Value).SelectMany(x => x).ToList(),
            }, default);
        };
    }

    public async Task AddLines(Guid clientId, IEnumerable<Line> lines, CancellationToken ct)
    {
        var user = this.userManagementService.GetUserFromConnectionId(clientId);
        if (!this.lines.TryGetValue(user.Name, out var clientLines))
        {
            var lineCollection = new ConcurrentBag<Line>();
            clientLines = this.lines.AddOrUpdate(user.Name, lineCollection, (_, value) => lineCollection);
        }

        foreach (var item in lines)
        {
            clientLines.Add(item);

        }

        Console.WriteLine("Lines received: " + string.Join(";", lines.Select(x => $"X:{x.Start.X},Y:{x.Start.Y}")));
        Console.WriteLine("Total Lines from client: " + clientLines.Count);
        var message = this.tcpService.CreateMessage(new WhiteboardAddLineMessage()
        {
            Lines = lines.ToList(),
        });

        foreach (var item in this.tcpService.Clients)
        {
            if (item == clientId)
            {
                continue;
            }

            await this.tcpService.SendMessage(item, message, ct);
        }

    }
}
