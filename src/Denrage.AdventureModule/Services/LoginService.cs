using Denrage.AdventureModule.Libs.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Services
{
    public class LoginService : IDisposable
    {
        private readonly TcpService tcpService;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public event Action LoggedIn;

        public string Name { get; } = "TestUser";

        public LoginService(TcpService tcpService)
        {
            this.tcpService = tcpService;
            this.tcpService.Connected += this.Login;
        }

        public void Dispose() => this.cancellationTokenSource.Cancel();

        public async void Login()
        {
            var result = await this.tcpService.SendAndAwaitAnswer<LoginMessage, LoginResponseMessage>(new LoginMessage() { Name = this.Name }, this.cancellationTokenSource.Token);

            if (result.Success)
            {
                this.LoggedIn?.Invoke();
            }
        }
    }
}
