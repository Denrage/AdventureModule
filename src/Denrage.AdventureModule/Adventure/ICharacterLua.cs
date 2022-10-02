using Microsoft.Xna.Framework;
using Neo.IronLua;
using System;

namespace Denrage.AdventureModule.Adventure
{
    public interface ICharacterLua
    {
        event Func<string, LuaResult> EmoteUsed;

        Vector3 Position { get; }
    }


}


