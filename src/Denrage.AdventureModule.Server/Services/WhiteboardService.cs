using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using System.Collections.Concurrent;

namespace Denrage.AdventureModule.Server.Services;

public class WhiteboardService
{
    private readonly Func<TcpService> getTcpService;
    private readonly ConcurrentDictionary<Guid, ConcurrentBag<Line>> lines = new();

    private TcpService tcpService;

    public WhiteboardService(Func<TcpService> getTcpService)
    {
        this.getTcpService = getTcpService;
    }

    private TcpService GetTcpService()
    {
        if (tcpService == null)
        {
            tcpService = getTcpService();
        }

        return tcpService;
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
        var message = this.GetTcpService().CreateMessage(new WhiteboardAddLineMessage()
        {
            Lines = lines.ToList(),
        });

        foreach (var item in this.GetTcpService().Clients)
        {
            if (item == clientId)
            {
                continue;
            }

            await this.GetTcpService().SendMessage(item, message, ct);
        }

    }
}
