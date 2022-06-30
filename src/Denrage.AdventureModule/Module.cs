using Blish_HUD;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Services;
using Denrage.AdventureModule.UserInterface.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Denrage.AdventureModule
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module
    {

        public static readonly Logger Logger = Logger.GetLogger<Module>();
        private readonly DrawObjectService drawObjectService;
        private readonly TcpService tcpService;
        private readonly PlayerMumbleService playerMumbleService;
        private readonly LoginService loginService;
        private readonly Dictionary<string, (MapMarker MapMarker, PlayerMarker PlayerMarker)> playerMarkers = new Dictionary<string, (MapMarker, PlayerMarker)>();
        private MapMarker mapMarker;
        private SettingEntry<KeyBinding> placeMapMarkerKeybind;

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
            this.tcpService = new TcpService(() => this.drawObjectService, () => this.loginService, () => this.playerMumbleService);
            this.loginService = new LoginService(this.tcpService);
            this.playerMumbleService = new PlayerMumbleService(this.tcpService, this.loginService);
            this.drawObjectService = new DrawObjectService(this.tcpService);
            this.drawObjectService.Register<Line, AddDrawObjectMessage<Line>, RemoveDrawObjectMessage<Line>>(lines => new AddDrawObjectMessage<Line>() { DrawObjects = lines.ToArray() }, ids => new RemoveDrawObjectMessage<Line>() { Ids = ids.ToArray() });
            this.drawObjectService.Register<Libs.Messages.Data.MapMarker, AddDrawObjectMessage<Libs.Messages.Data.MapMarker>, RemoveDrawObjectMessage<Libs.Messages.Data.MapMarker>>(marker => new AddDrawObjectMessage<Libs.Messages.Data.MapMarker>() { DrawObjects = marker.ToArray() }, ids => new RemoveDrawObjectMessage<Libs.Messages.Data.MapMarker>() { Ids = ids.ToArray() });
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            this.placeMapMarkerKeybind = settings.DefineSetting("DefaultMountBinding", new KeyBinding(Keys.L), () => "Map Marker", () => "");
            this.placeMapMarkerKeybind.Value.Enabled = true;
            this.placeMapMarkerKeybind.Value.Activated += async delegate { await PlaceMapMarker(); };
        }

        private Task PlaceMapMarker()
        {
            if (!GameService.Gw2Mumble.IsAvailable)
            {
                return Task.CompletedTask;
            }

            if (!GameService.Gw2Mumble.UI.IsMapOpen)
            {
                return Task.CompletedTask;
            }

            var mousePosition = GameService.Input.Mouse.Position;

            this.drawObjectService.Add(new[] {new MapMarker
            {
                Position = this.ScreenToContinentCoords(mousePosition.ToVector2()).ToMessageVector(),
                Username = this.loginService.Name,
                Id = Guid.NewGuid(),
            } }, false, default);

            return Task.CompletedTask;
        }

        private Microsoft.Xna.Framework.Vector2 ScreenToContinentCoords(Microsoft.Xna.Framework.Vector2 screenCoordinates)
        {
            var mapCenter = GameService.Gw2Mumble.UI.MapCenter;
            var scale = (float)GameService.Gw2Mumble.UI.MapScale * 0.897f;
            var boundsCenter = GameService.Graphics.SpriteScreen.Size.ToVector2() / 2f;
            return ((screenCoordinates - boundsCenter) * scale) + new Microsoft.Xna.Framework.Vector2((float)mapCenter.X, (float)mapCenter.Y);
        }

        protected override void Initialize()
        {

        }

        protected override async Task LoadAsync()
        {
            GameService.Graphics.World.AddEntity(new TestEntity());
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

            var window = new CanvasWindow()
            {
                Parent = GraphicsService.Graphics.SpriteScreen,
            };
            window.Initialize(this.drawObjectService, this.loginService);
            window.Show();

            var markerContainer = new MapMarkerContainer(this.drawObjectService)
            {
                Parent = GameService.Graphics.SpriteScreen,
            };

            //var window = new ImageWindow(this.ContentsManager)
            //{
            //    Parent = GraphicsService.Graphics.SpriteScreen,
            //    Width = 800,
            //    Height = 600,
            //    Location = new Point(400, 400),
            //};
        }

        protected override void OnModuleLoaded(EventArgs e)
        {

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GameService.Gw2Mumble.IsAvailable)
            {
                foreach (var player in this.playerMumbleService.OtherPlayerInformation)
                {
                    if (!this.playerMarkers.TryGetValue(player.Key, out var marker))
                    {
                        marker = (null, new PlayerMarker());
                        GameService.Graphics.World.AddEntity(marker.PlayerMarker);
                        this.playerMarkers[player.Key] = marker;
                    }

                    marker.PlayerMarker.Position = player.Value.MapPosition.ToVector();
                    //marker.MapMarker.Marker.Position = player.Value.ContinentPosition;
                }
            }
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here

            // All static members must be manually unset
        }

    }
}
