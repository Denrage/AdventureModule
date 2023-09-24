using System;

namespace Denrage.AdventureModule.Interfaces
{
    public interface IInitializationService
    {
        event Action Initialize;

        event Action Finalize;

        bool IsInitialized { get; set; }
    }
}