using Grpc.Core;
using GrpcWhiteboard;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Denrage.AdventureModule.Server;

public class TcpServer
{
    private readonly TcpListener listener;
    private readonly ConcurrentDictionary<Guid, TcpClientContext> clients = new();

    public List<Guid> Clients => this.clients.Keys.ToList();

    public event Action<Guid, byte[]> DataReceived;

    public event Action<Guid> ClientConnected;

    public TcpServer()
    {
        this.listener = new TcpListener(System.Net.IPAddress.Any, 5837);
    }

    public async Task Run(CancellationToken ct)
    {
        Console.WriteLine("Start listening");
        this.listener.Start();
        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            var internalClient = new TcpClientContext(new Libs.TcpClient(client, ct));
            internalClient.DataReceived += this.DataReceived;
            if (!clients.TryAdd(internalClient.Id, internalClient))
            {
                client.Dispose();
                continue;
            }
            Console.WriteLine("Client connected: " + client.Client.RemoteEndPoint?.ToString() ?? string.Empty);
            this.ClientConnected?.Invoke(internalClient.Id);
        }
    }

    public async Task SendData(Guid id, string data, CancellationToken ct)
    {
        var client = this.clients[id];
        await client.Client.Send(data, ct);
    }

    private class TcpClientContext
    {
        public Libs.TcpClient Client { get; set; }

        public Guid Id { get; set; }

        public event Action<Guid, byte[]> DataReceived;

        public TcpClientContext(Libs.TcpClient tcpClient)
        {
            this.Client = tcpClient;
            this.Client.DataReceived += data => this.DataReceived?.Invoke(this.Id, data);
            this.Id = Guid.NewGuid();
        }
    }
}