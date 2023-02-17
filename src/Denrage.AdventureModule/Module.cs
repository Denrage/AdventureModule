using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Denrage.AdventureModule.Adventure;
using Denrage.AdventureModule.Helper;
using Denrage.AdventureModule.Interfaces.Mumble;
using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Mock;
using Denrage.AdventureModule.Services;
using Denrage.AdventureModule.UserInterface;
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
        private readonly SynchronizationService synchronizationService;
        private readonly TcpService tcpService;
        private readonly PlayerMumbleService playerMumbleService;
        private readonly LoginService loginService;
        private readonly IGw2Mumble gw2Mumble;
        private SettingEntry<KeyBinding> placeMapMarkerKeybind;
        private AdventureScript adventureScript;
        private SettingEntry<KeyBinding> emoteKeybind;

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
            this.gw2Mumble = new Gw2MumbleWrapper();
            //this.gw2Mumble = GameService.Gw2Mumble.IsAvailable ? new Gw2MumbleWrapper() : (IGw2Mumble)new MockMumble();

            this.tcpService = new TcpService(() => this.drawObjectService, () => this.synchronizationService, () => this.loginService, () => this.playerMumbleService);
            this.loginService = new LoginService(this.tcpService);
            this.playerMumbleService = new PlayerMumbleService(this.tcpService, this.loginService, this.gw2Mumble);
            this.drawObjectService = new DrawObjectService(this.tcpService);
            
            this.drawObjectService.Register<Line, AddDrawObjectMessage<Line>, RemoveDrawObjectMessage<Line>, UpdateDrawObjectMessage<Line>>(
                lines => new AddDrawObjectMessage<Line>() { DrawObjects = lines.ToArray() }, 
                ids => new RemoveDrawObjectMessage<Line>() { Ids = ids.ToArray() }, 
                lines => new UpdateDrawObjectMessage<Line>() { DrawObjects = lines.ToArray() },
                (oldObject, newObject) =>
                {
                    oldObject.Start = newObject.Start;
                    oldObject.End = newObject.End;
                    oldObject.LineColor = newObject.LineColor;
                    oldObject.TimeStamp = newObject.TimeStamp;
                });
            
            this.drawObjectService.Register<Libs.Messages.Data.MapMarker, AddDrawObjectMessage<Libs.Messages.Data.MapMarker>, RemoveDrawObjectMessage<Libs.Messages.Data.MapMarker>, UpdateDrawObjectMessage<Libs.Messages.Data.MapMarker>>(
                marker => new AddDrawObjectMessage<Libs.Messages.Data.MapMarker>() { DrawObjects = marker.ToArray() }, 
                ids => new RemoveDrawObjectMessage<Libs.Messages.Data.MapMarker>() { Ids = ids.ToArray() }, 
                marker => new UpdateDrawObjectMessage<Libs.Messages.Data.MapMarker>() { DrawObjects = marker.ToArray() },
                (oldObject, newObject) => oldObject.Position = newObject.Position);

            this.drawObjectService.Register<Libs.Messages.Data.Image, AddDrawObjectMessage<Libs.Messages.Data.Image>, RemoveDrawObjectMessage<Libs.Messages.Data.Image>, UpdateDrawObjectMessage<Libs.Messages.Data.Image>>(
                images => new AddDrawObjectMessage<Libs.Messages.Data.Image>() { DrawObjects = images.ToArray() }, 
                ids => new RemoveDrawObjectMessage<Libs.Messages.Data.Image>() { Ids = ids.ToArray() }, 
                images => new UpdateDrawObjectMessage<Libs.Messages.Data.Image>() { DrawObjects = images.ToArray() },
                (oldObject, newObject) =>
                {
                    if (newObject.Location != null)
                    {
                        oldObject.Location = newObject.Location;
                    }

                    if (newObject.Data != null && newObject.Data.Length > 0)
                    {
                        oldObject.Data = newObject.Data;
                    }

                    if (newObject.Opacity.HasValue)
                    {
                        oldObject.Opacity = newObject.Opacity;
                    }

                    if (newObject.Rotation.HasValue)
                    {
                        oldObject.Rotation = newObject.Rotation;
                    }

                    if (newObject.Size != null)
                    {
                        oldObject.Size = newObject.Size;
                    }
                });

            this.synchronizationService = new SynchronizationService(this.tcpService);
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            this.placeMapMarkerKeybind = settings.DefineSetting("DefaultMountBinding", new KeyBinding(Keys.L), () => "Map Marker", () => "");
            this.placeMapMarkerKeybind.Value.Enabled = true;
            this.placeMapMarkerKeybind.Value.Activated += async delegate { await PlaceMapMarker(); };

            this.emoteKeybind = settings.DefineSetting("EmoteBinding", new KeyBinding(Keys.K), () => "Emote", () => "");
            this.emoteKeybind.Value.Enabled = true;
            this.emoteKeybind.Value.Activated += delegate { this.adventureScript.EmoteUsed(); };
        }

        private async Task PlaceMapMarker()
        {
            if (!GameService.Gw2Mumble.IsAvailable)
            {
                return;
            }

            if (!this.gw2Mumble.UserInterface.IsMapOpen)
            {
                return;
            }

            var mousePosition = GameService.Input.Mouse.Position;

            var existingMarker = this.GetUserMarker(mousePosition.ToVector2());

            if (existingMarker != null)
            {
                await this.drawObjectService.Remove<MapMarker>(new[] { existingMarker.Id }, false, default);
            }
            else
            {
                await this.drawObjectService.Add(new[] {new MapMarker
                {
                    Position = MapUtils.ScreenToContinentCoords(mousePosition.ToVector2(), this.gw2Mumble).ToMessageVector(),
                    Username = this.loginService.Name,
                    Id = Guid.NewGuid(),
                } }, false, default);
            }
        }

        private MapMarker GetUserMarker(Microsoft.Xna.Framework.Vector2 screenPosition)
        {
            var x = screenPosition.X - 10;
            var y = screenPosition.Y - 10;
            var maxX = screenPosition.X + 10;
            var maxY = screenPosition.Y + 10;

            var topLeft = MapUtils.ScreenToContinentCoords(new Microsoft.Xna.Framework.Vector2(x, y), this.gw2Mumble);
            var bottomRight = MapUtils.ScreenToContinentCoords(new Microsoft.Xna.Framework.Vector2(maxX, maxY), this.gw2Mumble);

            foreach (var marker in this.drawObjectService.GetDrawObjects<MapMarker>())
            {
                if (marker.Position.X > topLeft.X && marker.Position.X < bottomRight.X && marker.Position.Y > topLeft.Y && marker.Position.Y < bottomRight.Y)
                {
                    return marker;
                }
            }

            return null;
        }

        protected override void Initialize()
        {

        }

        protected override async Task LoadAsync()
        {
            GameService.Graphics.World.AddEntity(new Entities.MarkerEntity(this.ContentsManager.GetTexture("marker.png"), this.gw2Mumble));
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
            //window.Initialize(this.drawObjectService, this.loginService);
            //window.Show();

            var markerContainer = new MapMarkerContainer(this.drawObjectService, this.gw2Mumble);
            var playerMarker = new PlayerMarkerControl(this.playerMumbleService, this.gw2Mumble);

            //var window2 = new ImageWindow(this.ContentsManager.GetTexture("testimage.jpg"))
            //{
            //    Parent = GraphicsService.Graphics.SpriteScreen,
            //    Width = 800,
            //    Height = 600,
            //    Location = new Point(400, 400),
            //};

            //var window2 = new ImageViewerWindow()
            //{
            //    Parent = GraphicsService.Graphics.SpriteScreen,
            //    Width = 100,
            //    Height = 800,
            //    Location = new Point(400, 400),
            //};

            //window2.Initialize();
            //window2.Show();
            var dialog = new DialogBuilder(this.synchronizationService);
            this.adventureScript = new AdventureScript(dialog, this.synchronizationService, this.gw2Mumble);
            //File.WriteAllText("mumble.json", Newtonsoft.Json.JsonConvert.SerializeObject(GameService.Gw2Mumble.RawClient));
        }

        protected override void Update(GameTime gameTime)
        {
            this.adventureScript?.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here

            // All static members must be manually unset
        }
    }
}
