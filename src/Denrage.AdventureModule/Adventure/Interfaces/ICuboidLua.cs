using System;

namespace Denrage.AdventureModule.Adventure.Interfaces
{
    public interface ICuboidLua
    {
        event Action PlayerEntered;

        bool CharacterInside { get; }

        bool IsCharacterInside(string name);

        void Test();
    }
}

