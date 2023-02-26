using Blish_HUD;
using Denrage.AdventureModule.Adventure.Interfaces;
using Denrage.AdventureModule.Interfaces.Mumble;
using Microsoft.Xna.Framework;
using Neo.IronLua;
using System;

namespace Denrage.AdventureModule.Adventure.Elements
{
    public class CharacterInformation : ICharacterLua
    {
        private readonly IGw2Mumble gw2Mumble;

        public Vector3 Position => gw2Mumble.PlayerCharacter.Position;

        public event Func<string, LuaResult> EmoteUsed;

        public CharacterInformation(IGw2Mumble gw2Mumble)
        {
            this.gw2Mumble = gw2Mumble;
        }

        public void FireEmoteUsed()
        {
            this.EmoteUsed?.Invoke("hello world");
        }
    }
}

