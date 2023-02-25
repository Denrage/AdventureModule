using Denrage.AdventureModule.Adventure;
using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.MessageHandlers;
using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<object>> responseTasks = new ConcurrentDictionary<Guid, TaskCompletionSource<object>>();
        private CancellationTokenSource cancellationTokenSource;
        private bool isConnected = false;

        public bool IsConnected => this.isConnected && (this.client?.IsConnected ?? false);

        public event Action Disconnected;

        public event Action ConnectionLossed;

        public event Action Connected;

        public TcpService(Func<DrawObjectService> drawObjectService, Func<SynchronizationService> getSynchronizationService, Func<LoginService> loginService, Func<PlayerMumbleService> playerMumbleService)
        {
            this.messageTypes = new Dictionary<string, (Type, MessageHandler)>()
            {
                { typeof(AddDrawObjectMessage<Line>).FullName, (typeof(AddDrawObjectMessage<Line>), new AddDrawObjectMessageHandler<Line>(drawObjectService)) },
                { typeof(RemoveDrawObjectMessage<Line>).FullName, (typeof(RemoveDrawObjectMessage<Line>), new RemoveDrawObjectMessageHandler<Line>(drawObjectService)) },
                { typeof(AddDrawObjectMessage<MapMarker>).FullName, (typeof(AddDrawObjectMessage<MapMarker>), new AddDrawObjectMessageHandler<MapMarker>(drawObjectService)) },
                { typeof(RemoveDrawObjectMessage<MapMarker>).FullName, (typeof(RemoveDrawObjectMessage<MapMarker>), new RemoveDrawObjectMessageHandler<MapMarker>(drawObjectService)) },
                { typeof(AddDrawObjectMessage<Image>).FullName, (typeof(AddDrawObjectMessage<Image>), new AddDrawObjectMessageHandler<Image>(drawObjectService)) },
                { typeof(RemoveDrawObjectMessage<Image>).FullName, (typeof(RemoveDrawObjectMessage<Image>), new RemoveDrawObjectMessageHandler<Image>(drawObjectService)) },
                { typeof(UpdateDrawObjectMessage<Image>).FullName, (typeof(UpdateDrawObjectMessage<Image>), new UpdateDrawObjectMessageHandler<Image>(drawObjectService)) },
                { typeof(PlayersMumbleMessage).FullName, (typeof(PlayersMumbleMessage), new PlayersMumbleMessageHandler(loginService, playerMumbleService)) },
                { typeof(PingResponseMessage).FullName, (typeof(PingResponseMessage), null) },
                { typeof(LoginResponseMessage).FullName, (typeof(LoginResponseMessage), null) },
                { typeof(StateChangedMessage<DialogState>).FullName, (typeof(StateChangedMessage<DialogState>), new StateChangedMessageHandler<DialogState>(getSynchronizationService)) },
                { typeof(StateChangedMessage<LuaVariablesState>).FullName, (typeof(StateChangedMessage<LuaVariablesState>), new StateChangedMessageHandler<LuaVariablesState>(getSynchronizationService)) },
                { typeof(StateChangedMessage<AdventureState>).FullName, (typeof(StateChangedMessage<AdventureState>), new StateChangedMessageHandler<AdventureState>(getSynchronizationService)) },
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

        public void Disconnect()
        {
            this.cancellationTokenSource.Cancel();
            
            if (client != null)
            {
                this.client.Disconnect();
                this.client.DataReceived -= this.Handle;
                this.client = null;
            }
        }

        private async void Handle(byte[] data)
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            var tcpMessage = System.Text.Json.JsonSerializer.Deserialize<TcpMessage>(json);
            var message = (Message)Newtonsoft.Json.JsonConvert.DeserializeObject(tcpMessage.Data, this.messageTypes[tcpMessage.TypeIdentifier].Type, new Newtonsoft.Json.JsonSerializerSettings()
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
            });

            if (message is ResponseMessage)
            {
                if (this.responseTasks.TryRemove(message.Id, out var completionSource))
                {
                    completionSource.SetResult(message);
                }

                // If no one waits for this, just ignore it
                return;
            }

            await this.messageTypes[tcpMessage.TypeIdentifier].Handler.Handle(default, message, this.cancellationTokenSource.Token);
        }

        private async Task Heartbeat(CancellationToken ct)
        {
            while (true)
            {
                await Task.Delay(5000, ct);
                var loopCancellationToken = new CancellationTokenSource();
                var combinedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(ct, loopCancellationToken.Token);
                var sendTask = this.SendAndAwaitAnswer<PingMessage, PingResponseMessage>(new PingMessage(), combinedCancellationToken.Token);
                var taskResult = await Task.WhenAny(sendTask, Task.Delay(TimeSpan.FromSeconds(20), combinedCancellationToken.Token));
                combinedCancellationToken.Cancel();
                if (taskResult != sendTask)
                {
                    this.cancellationTokenSource.Cancel();
                    this.client.Disconnect();
                    this.client.DataReceived -= this.Handle;
                    this.client = null;
                    this.ConnectionLossed?.Invoke();
                    return;
                }
            }
        }

        public async Task Send<T>(T message, CancellationToken ct)
            where T : Message
        {
            if (this.client == null || !this.client.IsConnected)
            {
                return;
            }

            if (message.Id == default)
            {
                message.Id = Guid.NewGuid();
            }

            var jsonOptions = new Newtonsoft.Json.JsonSerializerSettings()
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
            };

            var tcpMessage = new TcpMessage
            {
                TypeIdentifier = message.GetType().FullName,
                Data = Newtonsoft.Json.JsonConvert.SerializeObject(message, jsonOptions),
            };

            var data = System.Text.Json.JsonSerializer.Serialize(tcpMessage);

            await this.client.Send(System.Text.Encoding.UTF8.GetBytes(data), ct);
        }

        public async Task<TResponse> SendAndAwaitAnswer<TRequest, TResponse>(TRequest request, CancellationToken ct)
            where TRequest : Message
            where TResponse : Message
        {
            var completionSource = new TaskCompletionSource<object>();

            request.Id = Guid.NewGuid();
            if (!this.responseTasks.TryAdd(request.Id, completionSource))
            {
                throw new InvalidOperationException("This should never happen");
            }

            await this.Send(request, ct);

            return (TResponse)await completionSource.Task;
        }
    }
}
