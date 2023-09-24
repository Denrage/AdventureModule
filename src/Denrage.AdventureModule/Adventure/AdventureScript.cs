using Denrage.AdventureModule.Adventure.Elements;
using Denrage.AdventureModule.Adventure.Services;
using Denrage.AdventureModule.Adventure.Windows;
using Denrage.AdventureModule.Interfaces;
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
        private readonly DialogBuilder dialogBuilder;
        private readonly SynchronizationService synchronizationService;
        private readonly IInitializationService initializationService;
        private readonly TcpService tcpService;

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

        public AdventureScript(DialogBuilder dialogBuilder, SynchronizationService synchronizationService, IInitializationService initializationService, TcpService tcpService, IGw2Mumble gw2Mumble)
        {
            this.luaEngine = new Lua();

            this.logicCreator = new LogicBuilderCreator();
            this.logger = new LuaLogger();
            this.creator = new AdventureElementCreator(new AdventureDebugService(), gw2Mumble);
            this.characterInformation = new CharacterInformation(gw2Mumble);
            this.dialogBuilder = dialogBuilder;
            this.synchronizationService = synchronizationService;
            this.initializationService = initializationService;
            this.tcpService = tcpService;
        }

        public void Initialize()
        {
            var adventure = new Adventure(this.luaEngine, @"D:\Repos\AdventureModule\Adventure2", this.characterInformation, this.creator, this.logger, this.logicCreator, this.dialogBuilder, this.synchronizationService, this.initializationService, this.tcpService);
            var scriptWindow = new StepOverviewWindow(adventure, this.initializationService);
            scriptWindow.Show();
        }
    }
}
