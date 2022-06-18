using Denrage.AdventureModule.Libs.Messages;
using Grpc.Core;
using GrpcWhiteboard;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Denrage.AdventureModule.Server;

public class TcpService
{
    private readonly TcpServer server;
    private readonly Dictionary<string, (Type Type, MessageHandler Handler)> messageTypes;

    public List<Guid> Clients => this.server.Clients;

    public TcpService(WhiteboardService whiteboardService)
    {
        this.server = new TcpServer();
        this.server.DataReceived += this.DataReceived;

        this.messageTypes = new Dictionary<string, (Type, MessageHandler)>()
        {
            { typeof(WhiteboardAddLineMessage).Name, (typeof(WhiteboardAddLineMessage), new WhiteboardAddLineMessageHandler(whiteboardService)) },
        };
    }

    public async Task Initialize()
    {
        _ = Task.Run(async () => await this.server.Run());
    }

    public async Task SendMessage<T>(Guid clientId, T message)
        => await this.SendMessage(clientId, this.CreateMessage(message));


    public async Task SendMessage(Guid clientId, TcpMessage tcpMessage)
    {
        var data = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(tcpMessage));
        await this.server.SendData(clientId, data);
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
        await this.messageTypes[tcpMessage.TypeIdentifier].Handler.Handle(clientId, message);
    }
}

public abstract class MessageHandler
{
    public abstract Task Handle(Guid clientId, object message);
}

public abstract class MessageHandler<T> : MessageHandler
    where T : Message
{
    public override async Task Handle(Guid clientId, object message)
        => await this.Handle(clientId, (T)message);

    protected abstract Task Handle(Guid clientId, T message);
}

public class WhiteboardAddLineMessageHandler : MessageHandler<WhiteboardAddLineMessage>
{
    private readonly WhiteboardService whiteboardService;

    public WhiteboardAddLineMessageHandler(WhiteboardService whiteboardService)
    {
        this.whiteboardService = whiteboardService;
    }

    protected override async Task Handle(Guid clientId, WhiteboardAddLineMessage message)
    {
        await this.whiteboardService.AddLines(clientId, message.Lines);
    }
}

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

    public async Task AddLines(Guid clientId, IEnumerable<Line> lines)
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

            await this.GetTcpService().SendMessage(item, message);
        }

    }
}

public class TcpServer
{
    private readonly TcpListener listener;
    private readonly ConcurrentDictionary<Guid, Client> clients = new ConcurrentDictionary<Guid, Client>();

    public List<Guid> Clients => this.clients.Keys.ToList();

    public Action<Guid, byte[]> DataReceived;

    public TcpServer()
    {
        this.listener = new TcpListener(System.Net.IPAddress.Any, 5837);
    }

    public async Task Run()
    {
        Console.WriteLine("Start listening");
        this.listener.Start();
        while (true)
        {
            //await Task.Delay(TimeSpan.FromMilliseconds(100));

            var client = await listener.AcceptTcpClientAsync();
            var internalClient = new Client(client);
            internalClient.DataReceived += this.DataReceived;
            if (!clients.TryAdd(internalClient.Id, internalClient))
            {
                client.Dispose();
            }
            Console.WriteLine("Client connected: " + client.Client.RemoteEndPoint?.ToString() ?? string.Empty);
        }
    }

    public async Task SendData(Guid id, byte[] data)
    {
        var client = this.clients[id];
        await client.Write(data);
    }



    private class Client
    {
        private readonly Task receiveTask;

        public Guid Id { get; }

        public TcpClient TcpClient { get; }

        public NetworkStream NetworkStream { get; }

        public Action<Guid, byte[]> DataReceived;

        public Client(TcpClient client)
        {
            this.Id = Guid.NewGuid();
            this.TcpClient = client;
            this.NetworkStream = this.TcpClient.GetStream();
            this.receiveTask = Task.Run(async () => await this.Receive(this.TcpClient, this.NetworkStream));
        }

        public async Task Write(byte[] data)
        {
            if (this.TcpClient.Connected)
            {
                using var memoryStream = new MemoryStream(data);
                await memoryStream.CopyToAsync(this.NetworkStream);
            }
        }

        private async Task Receive(TcpClient client, NetworkStream networkStream)
        {
            Console.WriteLine($"Start Receive Task for {client.Client.RemoteEndPoint?.ToString()}");
            try
            {
                while (client.Connected)
                {
                    if (client.Available == 0)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(100));
                    }

                    using var memoryStream = new MemoryStream();
                    var bytesRead = -1;
                    var buffer = new byte[4096];
                    while (bytesRead != 0)
                    {
                        bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                        await memoryStream.WriteAsync(buffer, 0, bytesRead);
                        if (bytesRead < buffer.Length)
                        {
                            break;
                        }
                    }

                    _ = memoryStream.Seek(0, SeekOrigin.Begin);
                    var data = memoryStream.ToArray();
                    _ = Task.Run(() => this.DataReceived?.Invoke(this.Id, data));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

            Console.WriteLine($"Client {client.Client.RemoteEndPoint?.ToString()} disconnected");
        }
    }
}