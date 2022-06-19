using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Denrage.AdventureModule.Services;
using Denrage.AdventureModule.UserInterface.Windows;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;

namespace Denrage.AdventureModule
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {

        public static readonly Logger Logger = Logger.GetLogger<Module>();
        private readonly WhiteboardService whiteboardService;
        private readonly TcpService tcpService;
        
        internal static Module Instance { get; private set; }

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            Instance = this;
            this.tcpService = new TcpService(() => this.whiteboardService);
            this.whiteboardService = new WhiteboardService(this.tcpService);
        }

        protected override void DefineSettings(SettingCollection settings)
        {

        }

        protected override void Initialize()
        {

        }

        protected override async Task LoadAsync()
        {
            //GameService.Graphics.World.AddEntity(new TestEntity());
            var window = new CanvasWindow()
            {
                Parent = GraphicsService.Graphics.SpriteScreen,
            };
            this.tcpService.Connected += () => Logger.Info("Connected");
            this.tcpService.Disconnected += async () =>
            {
                Logger.Info("Disconnected");

                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    Logger.Info("Trying to reconnect");
                    await this.tcpService.Initialize();
                    if (this.tcpService.IsConnected)
                    {
                        Logger.Info("Reconnected");
                        return;
                    }
                }
            };
            await this.tcpService.Initialize();
            window.Initialize(this.whiteboardService);
            window.Show();
        }

        protected override void OnModuleLoaded(EventArgs e)
        {

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime)
        {

        }

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here

            // All static members must be manually unset
        }

    }
}
