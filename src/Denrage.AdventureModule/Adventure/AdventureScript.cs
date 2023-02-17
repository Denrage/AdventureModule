using Denrage.AdventureModule.Interfaces.Mumble;
using Denrage.AdventureModule.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Neo.IronLua;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Adventure
{

    public class AdventureScript
    {
        private AdventureElementCreator creator;
        private readonly CharacterInformation characterInformation;
        private readonly LogicBuilderCreator logicCreator;
        private readonly LuaLogger logger;
        private readonly Lua luaEngine;

        public Dictionary<string, Step> Steps { get; } = new Dictionary<string, Step>();

        public void Update(GameTime gameTime)
        {
            var elements = this.creator.Elements;
            foreach (var item in elements)
            {
                item.Update(gameTime);
            }
        }

        public void EmoteUsed() 
            => this.characterInformation.FireEmoteUsed();

        public AdventureScript(DialogBuilder dialogBuilder, SynchronizationService synchronizationService, IGw2Mumble gw2Mumble)
        {
            this.luaEngine = new Lua();

            this.logicCreator = new LogicBuilderCreator();
            this.logger = new LuaLogger();
            this.creator = new AdventureElementCreator(new AdventureDebugService(), gw2Mumble);
            this.characterInformation = new CharacterInformation(gw2Mumble);
            var adventure = new Adventure(this.luaEngine, @"D:\Repos\AdventureModule\Adventure2", characterInformation, creator, logger, logicCreator, dialogBuilder, synchronizationService);
            var scriptWindow = new StepOverviewWindow(adventure);
            scriptWindow.Show();
        }
    }
}
