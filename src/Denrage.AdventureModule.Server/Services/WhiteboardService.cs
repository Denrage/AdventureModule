using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using System.Collections.Concurrent;

namespace Denrage.AdventureModule.Server.Services;

public class WhiteboardService
{
    private readonly ConcurrentDictionary<Guid, ConcurrentBag<Line>> lines = new();

    private TcpService tcpService;

    public WhiteboardService(TcpService tcpService)
    {
        this.tcpService = tcpService;
        this.tcpService.ClientConnected += id =>
        {
            _ = this.tcpService.SendMessage(id, new WhiteboardAddLineMessage()
            {
                Lines = this.lines.Values.SelectMany(x => x).ToList(),
            }, default);
        };
    }

    public async Task AddLines(Guid clientId, IEnumerable<Line> lines, CancellationToken ct)
    {
        if (!this.lines.TryGetValue(clientId, out var clientLines))
        {
            clientLines = this.lines.AddOrUpdate(clientId, new ConcurrentBag<Line>(), (_, value) => value);
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
