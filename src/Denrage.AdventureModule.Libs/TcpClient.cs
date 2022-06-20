using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Libs
{
    public class TcpClient
    {
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

        public async Task Send(byte[] data, CancellationToken ct)
        {
            try
            {
                

                if (this.Client.Connected)
                {
                    using (var memoryStream = new MemoryStream(BitConverter.GetBytes(data.Length).Concat(data).ToArray()))
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
                            var messages = await this.ExtractMessages(memoryStream);

                            foreach (var message in messages)
                            {
                                _ = Task.Run(() => this.DataReceived?.Invoke(message));
                            }
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

        private async Task<IEnumerable<byte[]>> ExtractMessages(MemoryStream memoryStream)
        {
            var messages = new List<byte[]>();
            while (true)
            {
                if (memoryStream.Length == 0)
                {
                    // Exit loop when no data is left in memory stream
                    break;
                }

                _ = memoryStream.Seek(0, SeekOrigin.Begin);
                var lengthArray = new byte[sizeof(int)];
                var readBytes = await memoryStream.ReadAsync(lengthArray, 0, lengthArray.Length);

                if (readBytes != lengthArray.Length)
                {
                    throw new InvalidOperationException("Invalid data in stream!");
                }

                var messageLength = BitConverter.ToInt32(lengthArray, 0);
                if (memoryStream.Length - memoryStream.Position >= messageLength)
                {
                    var message = new byte[messageLength];
                    _ = await memoryStream.ReadAsync(message, 0, messageLength);
                    messages.Add(message);
                }
                else
                {
                    // Exit loop on incomplete message and wait for next buffer input
                    _ = memoryStream.Seek(0, SeekOrigin.End);
                    break;
                }

                var remainingDataLength = (int)memoryStream.Length - (int)memoryStream.Position;

                if (remainingDataLength > 0)
                {
                    var memoryStreamBuffer = memoryStream.GetBuffer();
                    Buffer.BlockCopy(memoryStreamBuffer, (int)memoryStream.Position, memoryStreamBuffer, 0, remainingDataLength);
                }

                _ = memoryStream.Seek(0, SeekOrigin.Begin);
                memoryStream.SetLength(remainingDataLength);
            }

            return messages;
        }
    }
}
