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
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public TcpService(WhiteboardService whiteboardService)
        {
            this.messageTypes = new Dictionary<string, (Type, MessageHandler)>()
            {
                { typeof(WhiteboardAddLineMessage).Name, (typeof(WhiteboardAddLineMessage), new WhiteboardAddLineMessageHandler(whiteboardService)) },
            };
        }

        public async Task Initialize()
        {
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("127.0.0.1", 5837);
            client = new Libs.TcpClient(tcpClient, this.cancellationTokenSource.Token);
            client.DataReceived += this.Handle;
        }

        private async void Handle(byte[] data)
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            var tcpMessage = System.Text.Json.JsonSerializer.Deserialize<TcpMessage>(json);
            var message = System.Text.Json.JsonSerializer.Deserialize(tcpMessage.Data, this.messageTypes[tcpMessage.TypeIdentifier].Type);
            await this.messageTypes[tcpMessage.TypeIdentifier].Handler.Handle(default, message, this.cancellationTokenSource.Token);
        }

        public async Task Send<T>(T message, CancellationToken ct)
            where T : Message
        {
            var tcpMessage = new TcpMessage();

            tcpMessage.TypeIdentifier = typeof(T).Name;
            tcpMessage.Data = System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message));

            var data = System.Text.Json.JsonSerializer.Serialize(tcpMessage);

            await this.client.Write(data, ct);
        }
    }
}
