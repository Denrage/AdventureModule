using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.MessageHandlers;

namespace Denrage.AdventureModule.Server.Services;

public class TcpService : IDisposable
{
    private readonly TcpServer server;
    private readonly Dictionary<string, (Type Type, MessageHandler Handler)> messageTypes;
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly UserManagementService userManagementService;

    public List<Guid> Clients => this.server.Clients;

    public event Action<Guid> ClientConnected;

    public TcpService(Func<DrawObjectService> getDrawObjectService, UserManagementService userManagementService, Func<PlayerMumbleService> playerMumbleService)
    {
        this.cancellationTokenSource = new CancellationTokenSource();
        this.server = new TcpServer();
        this.server.DataReceived += this.DataReceived;
        this.server.ClientConnected += id => this.ClientConnected?.Invoke(id);

        this.messageTypes = new Dictionary<string, (Type, MessageHandler)>()
        {
            { typeof(AddDrawObjectMessage<Libs.Messages.Data.Line>).FullName, (typeof(AddDrawObjectMessage<Libs.Messages.Data.Line>), new AddDrawObjectMessageHandler<Libs.Messages.Data.Line>(getDrawObjectService)) },
            { typeof(RemoveDrawObjectMessage<Libs.Messages.Data.Line>).FullName, (typeof(RemoveDrawObjectMessage<Libs.Messages.Data.Line>), new RemoveDrawObjectMessageHandler<Libs.Messages.Data.Line>(getDrawObjectService)) },
            { typeof(AddDrawObjectMessage<Libs.Messages.Data.MapMarker>).FullName, (typeof(AddDrawObjectMessage<Libs.Messages.Data.MapMarker>), new AddDrawObjectMessageHandler<Libs.Messages.Data.MapMarker>(getDrawObjectService)) },
            { typeof(RemoveDrawObjectMessage<Libs.Messages.Data.MapMarker>).FullName, (typeof(RemoveDrawObjectMessage<Libs.Messages.Data.MapMarker>), new RemoveDrawObjectMessageHandler<Libs.Messages.Data.MapMarker>(getDrawObjectService)) },
            { typeof(AddDrawObjectMessage<Libs.Messages.Data.Image>).FullName, (typeof(AddDrawObjectMessage<Libs.Messages.Data.Image>), new AddDrawObjectMessageHandler<Libs.Messages.Data.Image>(getDrawObjectService)) },
            { typeof(RemoveDrawObjectMessage<Libs.Messages.Data.Image>).FullName, (typeof(RemoveDrawObjectMessage<Libs.Messages.Data.Image>), new RemoveDrawObjectMessageHandler<Libs.Messages.Data.Image>(getDrawObjectService)) },
            { typeof(UpdateDrawObjectMessage<Libs.Messages.Data.Image>).FullName, (typeof(UpdateDrawObjectMessage<Libs.Messages.Data.Image>), new UpdateDrawObjectMessageHandler<Libs.Messages.Data.Image>(getDrawObjectService)) },
            { typeof(PlayerMumbleMessage).FullName, (typeof(PlayerMumbleMessage), new PlayerMumbleMessageHandler(playerMumbleService, userManagementService)) },
            { typeof(PingMessage).FullName, (typeof(PingMessage), new PingMessageHandler(this)) },
            { typeof(LoginMessage).FullName, (typeof(LoginMessage), new LoginMessageHandler(userManagementService, this)) },
        };
        this.userManagementService = userManagementService;
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
