using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Services
{
    public class WhiteboardService
    {
        private readonly Dictionary<Guid, Line> diffLines = new Dictionary<Guid, Line>();
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly TcpService tcpService;
        private readonly LoginService loginService;

        public ConcurrentDictionary<Guid, Line> ServerLines { get; private set; } = new ConcurrentDictionary<Guid, Line>();

        public ConcurrentDictionary<Guid, Line> UserLines { get; } = new ConcurrentDictionary<Guid, Line>();

        public WhiteboardService(TcpService tcpService, LoginService loginService)
        {
            this.tcpService = tcpService;
            this.loginService = loginService;
            this.loginService.LoggedIn += this.Initialize;
            this.tcpService.Disconnected += () => this.ServerLines.Clear();
        }

        private void Initialize()
        {
            lock (this.diffLines)
            {
                this.diffLines.Clear();
                foreach (var item in this.UserLines)
                {
                    this.diffLines.Add(item.Key, item.Value);
                }
            }

            Task.Run(async () => await this.LineTask());
        }

        public void AddServerLines(IEnumerable<Line> lines)
        {
            foreach (var line in lines)
            {
                this.ServerLines.TryAdd(line.Id, line);
            }
        }

        public void AddUserLines(IEnumerable<Line> lines)
        {
            if (this.UserLines.Count == 0)
            {
                foreach (var item in lines)
                {
                    this.UserLines.TryAdd(item.Id, item);
                }
            }
        }

        public void AddUserLine(Line line)
        {
            this.UserLines.TryAdd(line.Id, line);
            lock (this.diffLines)
            {
                this.diffLines.Add(line.Id, line);
            }
        }

        public void DeleteUserLines(IEnumerable<Line> lines)
        {
            foreach (var item in lines)
            {
                this.UserLines.TryRemove(item.Id, out _);
            }

            // TODO: Cancellationtoken
            Task.Run(async () => await this.tcpService.Send(new WhiteboardRemoveLineMessage() { Ids = lines.Select(x => x.Id).ToList() }, default));
        }

        public void DeleteServerLines(IEnumerable<Guid> lines)
        {
            foreach (var item in lines)
            {
                this.ServerLines.TryRemove(item, out _);
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
                    linesToSend = this.diffLines.Values.ToList();
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
