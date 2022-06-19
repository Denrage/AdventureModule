using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.MessageHandlers;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Services
{
    public class TcpService
    {
        private Libs.TcpClient client;
        private readonly Dictionary<string, (Type Type, MessageHandler Handler)> messageTypes;
        private CancellationTokenSource cancellationTokenSource;
        private TaskCompletionSource<bool> heartBeatCompletionSource;
        private bool isConnected = false;


        public bool IsConnected => this.isConnected && (this.client?.IsConnected ?? false);

        public event Action Disconnected;

        public event Action Connected;

        public TcpService(WhiteboardService whiteboardService)
        {
            this.messageTypes = new Dictionary<string, (Type, MessageHandler)>()
            {
                { typeof(WhiteboardAddLineMessage).Name, (typeof(WhiteboardAddLineMessage), new WhiteboardAddLineMessageHandler(whiteboardService)) },
                { typeof(PingMessage).Name, (typeof(PingMessage), new PingMessageHandler(this)) },
            };
        }

        public async Task Initialize()
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            var tcpClient = new TcpClient();
            try
            {
                await tcpClient.ConnectAsync("127.0.0.1", 5837);
            }
            catch (SocketException)
            {
                return;
            }
            isConnected = true;
            client = new Libs.TcpClient(tcpClient, this.cancellationTokenSource.Token);
            client.DataReceived += this.Handle;
            _ = Task.Run(async () => this.Heartbeat(this.cancellationTokenSource.Token));
            client.Disconnected += this.Disconnected;
            this.Connected?.Invoke();
        }

        private async void Handle(byte[] data)
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            var tcpMessage = System.Text.Json.JsonSerializer.Deserialize<TcpMessage>(json);
            var message = System.Text.Json.JsonSerializer.Deserialize(tcpMessage.Data, this.messageTypes[tcpMessage.TypeIdentifier].Type);
            await this.messageTypes[tcpMessage.TypeIdentifier].Handler.Handle(default, message, this.cancellationTokenSource.Token);
        }

        public void HeartbeatReceived()
        {
            this.heartBeatCompletionSource.SetResult(true);
        }

        private async Task Heartbeat(CancellationToken ct)
        {
            while (true)
            {
                await Task.Delay(5000, ct);
                this.heartBeatCompletionSource = new TaskCompletionSource<bool>();
                await this.Send(new PingMessage(), ct);
                if(!this.heartBeatCompletionSource.Task.Wait((int)TimeSpan.FromSeconds(5).TotalMilliseconds, ct))
                {
                    this.cancellationTokenSource.Cancel();
                    this.client.Disconnect();
                    this.client.DataReceived -= this.Handle;
                    this.client = null;
                    return;
                }
            }
        }

        public async Task Send<T>(T message, CancellationToken ct)
            where T : Message
        {
            if (!this.client.IsConnected)
            {
                return;
            }

            var tcpMessage = new TcpMessage();

            tcpMessage.TypeIdentifier = typeof(T).Name;
            tcpMessage.Data = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message));

            var data = System.Text.Json.JsonSerializer.Serialize(tcpMessage);

            await this.client.Write(data, ct);
        }
    }
}
