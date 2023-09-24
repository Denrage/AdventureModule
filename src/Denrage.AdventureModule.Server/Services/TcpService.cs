using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.MessageHandlers;

namespace Denrage.AdventureModule.Server.Services;

public class TcpService : ITcpService, IDisposable
{
    private readonly TcpServer server;
    private readonly Dictionary<string, (Type Type, IMessageHandler Handler)> messageTypes;
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly IUserManagementService userManagementService;

    public List<Guid> Clients => this.server.Clients;

    public event Action<Guid>? ClientConnected;

    public TcpService(IUserManagementService userManagementService)
    {
        this.cancellationTokenSource = new CancellationTokenSource();
        this.server = new TcpServer();
        this.server.DataReceived += this.DataReceived;
        this.server.ClientConnected += id => this.ClientConnected?.Invoke(id);

        this.messageTypes = new Dictionary<string, (Type, IMessageHandler)>()
        {
            
        };
        this.userManagementService = userManagementService;
    }

    public async Task Initialize() 
        => await this.server.Run(this.cancellationTokenSource.Token);

    public void RegisterMessage(IMessageHandler handler) 
        => this.messageTypes.Add(handler.MessageType.FullName!, (handler.MessageType, handler));

    public async Task SendMessage<T>(Guid clientId, T message, CancellationToken ct)
        => await this.SendMessage(clientId, this.CreateMessage(message), ct);

    public async Task SendMessage(Guid clientId, TcpMessage tcpMessage, CancellationToken ct)
    {
        var data = System.Text.Json.JsonSerializer.Serialize(tcpMessage);
        await this.server.SendData(clientId, System.Text.Encoding.UTF8.GetBytes(data), ct);
    }

    public async Task SendToGroup<T>(Guid clientId, T message, CancellationToken ct)
    {
        var group = this.userManagementService.GetGroup(this.userManagementService.GetUserFromConnectionId(clientId));
        foreach (var member in group.Users)
        {
            var connectionId = this.userManagementService.GetConnectionIdFromUser(member);
            if (connectionId != clientId)
            {
                await this.SendMessage(connectionId, message, ct);
            }
        }
    }

    public async Task SendToGroup<T>(Group group, T message, CancellationToken ct)
    {
        foreach (var user in group.Users)
        {
            await this.SendMessage(this.userManagementService.GetConnectionIdFromUser(user), message, ct);
        }
    }

    public TcpMessage CreateMessage<T>(T message)
        => new()
        {
            TypeIdentifier = message.GetType().FullName,
            Data = Newtonsoft.Json.JsonConvert.SerializeObject(message, new Newtonsoft.Json.JsonSerializerSettings()
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
            }),
        };

    private async void DataReceived(Guid clientId, byte[] data)
    {
        var json = System.Text.Encoding.UTF8.GetString(data);
        var tcpMessage = System.Text.Json.JsonSerializer.Deserialize<TcpMessage>(json);
        var message = Newtonsoft.Json.JsonConvert.DeserializeObject(tcpMessage.Data, this.messageTypes[tcpMessage.TypeIdentifier].Type, new Newtonsoft.Json.JsonSerializerSettings()
        {
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
        });
        await this.messageTypes[tcpMessage.TypeIdentifier].Handler.Handle(clientId, message, this.cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        this.cancellationTokenSource.Cancel();
    }
}
