using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using System.Collections.Concurrent;

namespace Denrage.AdventureModule.Server.Services;

public class WhiteboardService
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Line>> lines = new();

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
                Lines = this.lines.Where(x => x.Key != user.Name).Select(x => x.Value.Values).SelectMany(x => x).ToList(),
            }, default);
        };
    }

    public async Task AddLines(Guid clientId, IEnumerable<Line> lines, CancellationToken ct)
    {
        var user = this.userManagementService.GetUserFromConnectionId(clientId);
        if (!this.lines.TryGetValue(user.Name, out var clientLines))
        {
            var lineCollection = new ConcurrentDictionary<Guid, Line>();
            clientLines = this.lines.AddOrUpdate(user.Name, lineCollection, (_, value) => lineCollection);
        }

        foreach (var item in lines)
        {
            clientLines.TryAdd(item.Id, item);

        }

        Console.WriteLine("Lines received: " + string.Join(";", lines.Select(x => $"X:{x.Start.X},Y:{x.Start.Y}")));
        Console.WriteLine("Total Lines from client: " + clientLines.Count);

        await this.tcpService.SendToGroup(
            clientId, 
            new WhiteboardAddLineMessage()
            {
                Lines = lines.ToList(),
            }, 
            ct);
    }

    public async Task RemoveLines(Guid clientId, List<Guid> lines, CancellationToken ct)
    {
        var user = this.userManagementService.GetUserFromConnectionId(clientId);
        if (!this.lines.TryGetValue(user.Name, out var clientLines))
        {
            return;
        }

        foreach (var line in lines)
        {
            clientLines.TryRemove(line, out _);
        }

        await this.tcpService.SendToGroup(
            clientId,
            new WhiteboardRemoveLineMessage()
            {
                Ids = lines,
            }, 
            ct);

    }
}
