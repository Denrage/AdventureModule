using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Libs
{
    public class TcpClient
    {
        private const string EndOfMessageToken = "✷";
        private readonly NetworkStream networkStream;

        private bool isConnected = false;

        public bool IsConnected => this.isConnected && this.Client.Connected;

        public System.Net.Sockets.TcpClient Client { get; }

        public event Action<byte[]> DataReceived;

        public event Action Disconnected;

        public TcpClient(System.Net.Sockets.TcpClient client, CancellationToken ct)
        {
            this.Client = client;
            this.networkStream = this.Client.GetStream();
            this.isConnected = true;
            _ = Task.Run(async () => await this.Receive(ct), ct);
        }

        public async Task Send(string data, CancellationToken ct)
        {
            try
            {
                data += EndOfMessageToken;

                if (this.Client.Connected)
                {
                    using (var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data)))
                    {
                        await memoryStream.CopyToAsync(this.networkStream, 81920, ct);
                    }
                }
            }
            catch (SocketException)
            {
                this.Disconnect();
            }
        }

        public void Disconnect()
        {
            if (isConnected)
            {
                if (this.Client.Connected)
                {
                    this.Client.Close();
                    this.Client.Dispose();
                }

                this.isConnected = false;
                this.Disconnected?.Invoke();
            }
        }

        private async Task Receive(CancellationToken ct)
        {
            var messageToken = System.Text.Encoding.UTF8.GetBytes(EndOfMessageToken);
            Console.WriteLine($"Start Receive Task for {this.Client.Client.RemoteEndPoint?.ToString()}");
            try
            {
                while (this.Client.Connected)
                {
                    ct.ThrowIfCancellationRequested();
                    if (this.Client.Available == 0)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(100), ct);
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        var bytesRead = -1;
                        var buffer = new byte[4096];
                        while (bytesRead != 0)
                        {
                            bytesRead = await this.networkStream.ReadAsync(buffer, 0, buffer.Length, ct);
                            await memoryStream.WriteAsync(buffer, 0, bytesRead, ct);
                            await this.HandleMessages(messageToken, memoryStream, ct);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                this.Disconnect();
            }

            Console.WriteLine($"Client {this.Client.Client.RemoteEndPoint?.ToString()} disconnected");
        }

        private async Task HandleMessages(byte[] messageToken, MemoryStream memoryStream, CancellationToken ct)
        {
            var messageTokenPosition = -1;
            do
            {
                ct.ThrowIfCancellationRequested();
                var tempData = memoryStream.ToArray();
                messageTokenPosition = GetMessageTokenPosition(messageToken, tempData);

                if (messageTokenPosition != -1)
                {
                    _ = memoryStream.Seek(0, SeekOrigin.Begin);
                    var data = new byte[messageTokenPosition];
                    _ = await memoryStream.ReadAsync(data, 0, data.Length, ct);

                    _ = memoryStream.Seek(0, SeekOrigin.Begin);
                    var indexNextMessage = memoryStream.Length - (messageTokenPosition + messageToken.Length);
                    if (indexNextMessage != 0)
                    {
                        var remainingData = new byte[memoryStream.Length - indexNextMessage];
                        _ = memoryStream.Seek(messageTokenPosition + messageToken.Length, SeekOrigin.Begin);
                        _ = await memoryStream.ReadAsync(remainingData, 0, remainingData.Length, ct);
                        _ = memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.SetLength(0);
                        await memoryStream.WriteAsync(remainingData, 0, remainingData.Length, ct);
                    }
                    else
                    {
                        memoryStream.SetLength(0);
                    }

                    _ = Task.Run(() => this.DataReceived?.Invoke(data));
                }
            } while (messageTokenPosition != -1);
        }

        private static int GetMessageTokenPosition(byte[] messageToken, byte[] tempData)
        {
            var messageTokenPosition = -1;
            for (int i = 0; i < tempData.Length; i++)
            {
                if (tempData[i] == messageToken[0] && tempData.Length - i >= messageToken.Length)
                {
                    var setToken = true;
                    for (int j = 0; j < messageToken.Length; j++)
                    {
                        if (tempData[i + j] != messageToken[j])
                        {
                            setToken = false;
                        }
                    }

                    if (setToken)
                    {
                        messageTokenPosition = i;
                        break;
                    }
                }
            }

            return messageTokenPosition;
        }
    }
}
