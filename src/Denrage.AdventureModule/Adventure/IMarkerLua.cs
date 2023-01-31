using System;

namespace Denrage.AdventureModule.Adventure
{
    public interface IMarkerLua
    {
        event Action Interacted;

        void FlipNinetyDegrees();
    }
}

