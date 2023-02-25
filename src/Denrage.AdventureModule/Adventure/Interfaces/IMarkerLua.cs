using System;

namespace Denrage.AdventureModule.Adventure.Interfaces
{
    public interface IMarkerLua
    {
        event Action Interacted;

        void FlipNinetyDegrees();
    }
}

