using System;

namespace Denrage.AdventureModule.Adventure.Services
{
    public class AdventureDebugService
    {
        public event Action DebugActivated;

        public event Action DebugDeactivated;

        public bool IsDebug { get; private set; } = true;

        public void ToggleDebug()
        {
            if (this.IsDebug)
            {
                this.IsDebug = false;
                this.DebugDeactivated?.Invoke();
            }
            else
            {
                this.IsDebug = true;
                this.DebugActivated?.Invoke();
            }
        }
    }
}

