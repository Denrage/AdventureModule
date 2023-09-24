using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Denrage.AdventureModule.Interfaces;

namespace Denrage.AdventureModule.Services
{
    // This is so we can change a group assignment and let everything "reset" like it would be a reconnect, because changing the group is not a disconnect and the handling for that happens here
    public class InitializationService : IInitializationService
    {
        public event Action Initialize;

        public event Action Finalize;

        public bool IsInitialized { get; set; }

        public InitializationService(TcpService tcpService)
        {
            this.IsInitialized = tcpService.IsConnected;
            tcpService.Connected += () => this.Initialize?.Invoke();
            tcpService.Disconnected += () => this.Finalize?.Invoke();
        }
    }
}
