using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Services
{
    public class WhiteboardService
    {
        private readonly List<Line> diffLines = new List<Line>();
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly TcpService tcpService;

        public ConcurrentBag<Line> ServerLines { get; private set; } = new ConcurrentBag<Line>();

        public ConcurrentBag<Line> UserLines { get; } = new ConcurrentBag<Line>();

        public WhiteboardService(TcpService tcpService)
        {
            this.tcpService = tcpService;
            this.tcpService.Connected += this.Initialize;
            this.tcpService.Disconnected += () => this.ServerLines = new ConcurrentBag<Line>();
        }

        private void Initialize()
        {
            lock (this.diffLines)
            {
                this.diffLines.Clear();
                this.diffLines.AddRange(this.UserLines);
            }

            Task.Run(async () => await this.LineTask());
        }

        public void AddServerLines(IEnumerable<Line> lines)
        {
            foreach (var line in lines)
            {
                this.ServerLines.Add(line);
            }
        }

        public void AddUserLine(Line line)
        {
            this.UserLines.Add(line);
            lock (this.diffLines)
            {
                this.diffLines.Add(line);
            }
        }

        private async Task LineTask()
        {
            while (true)
            {
                this.cancellationTokenSource.Token.ThrowIfCancellationRequested();
                await Task.Delay(10, this.cancellationTokenSource.Token);
                var linesToSend = new List<Line>();
                lock (this.diffLines)
                {
                    linesToSend = this.diffLines.ToList();
                    this.diffLines.Clear();
                }

                if (linesToSend.Any())
                {
                    await this.tcpService.Send(new WhiteboardAddLineMessage()
                    {
                        Lines = linesToSend,
                    }, this.cancellationTokenSource.Token);
                }
            }
        }
    }
}
