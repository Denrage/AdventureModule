using Blish_HUD;
using Microsoft.Xna.Framework;
using Neo.IronLua;
using System;

namespace Denrage.AdventureModule.Adventure
{
    public class CharacterInformation : ICharacterLua
    {
        public Vector3 Position => GameService.Gw2Mumble.PlayerCharacter.Position;

        public event Func<string, LuaResult> EmoteUsed;

        public void FireEmoteUsed()
        {
            this.EmoteUsed?.Invoke("hello world");
        }
    }
}

