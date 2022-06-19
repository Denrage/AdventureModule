using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.MessageHandlers;

namespace Denrage.AdventureModule.Server.Services;

public class TcpService : IDisposable
{
    private readonly TcpServer server;
    private readonly Dictionary<string, (Type Type, MessageHandler Handler)> messageTypes;
    private readonly CancellationTokenSource cancellationTokenSource;

    public List<Guid> Clients => this.server.Clients;

    public TcpService(WhiteboardService whiteboardService)
    {
        this.cancellationTokenSource = new CancellationTokenSource();
        this.server = new TcpServer();
        this.server.DataReceived += this.DataReceived;

        this.messageTypes = new Dictionary<string, (Type, MessageHandler)>()
        {
            { typeof(WhiteboardAddLineMessage).Name, (typeof(WhiteboardAddLineMessage), new WhiteboardAddLineMessageHandler(whiteboardService)) },
        };
    }

    public async Task Initialize()
    {
        _ = Task.Run(async () => await this.server.Run(this.cancellationTokenSource.Token), this.cancellationTokenSource.Token);
    }

    public async Task SendMessage<T>(Guid clientId, T message, CancellationToken ct)
        => await this.SendMessage(clientId, this.CreateMessage(message), ct);


    public async Task SendMessage(Guid clientId, TcpMessage tcpMessage, CancellationToken ct)
    {
        var data = System.Text.Json.JsonSerializer.Serialize(tcpMessage);
        await this.server.SendData(clientId, data, ct);
    }

    public TcpMessage CreateMessage<T>(T message)
        => new()
        {
            TypeIdentifier = typeof(T).Name,
            Data = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message)),
        };

    private async void DataReceived(Guid clientId, byte[] data)
    {
        var json = System.Text.Encoding.UTF8.GetString(data);
        var tcpMessage = System.Text.Json.JsonSerializer.Deserialize<TcpMessage>(json);
        var message = System.Text.Json.JsonSerializer.Deserialize(tcpMessage.Data, this.messageTypes[tcpMessage.TypeIdentifier].Type);
        await this.messageTypes[tcpMessage.TypeIdentifier].Handler.Handle(clientId, message, this.cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        this.cancellationTokenSource.Cancel();
    }
}
