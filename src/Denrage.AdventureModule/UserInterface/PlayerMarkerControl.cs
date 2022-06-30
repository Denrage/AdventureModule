using Blish_HUD;
using Blish_HUD.Controls;
using Denrage.AdventureModule.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.UserInterface
{
    public class PlayerMarkerControl : Control
    {
        private readonly PlayerMumbleService playerMumbleService;
        private readonly Dictionary<string, PlayerMarker> playerMarkers = new Dictionary<string, PlayerMarker>();
        
        public Texture2D Texture { get; set; }

        public PlayerMarkerControl(PlayerMumbleService playerMumbleService)
        {
            this.playerMumbleService = playerMumbleService;
            this.ClipsBounds = false;
            this.Texture = Module.Instance.ContentsManager.GetTexture("marker.png");
            this.Parent = GameService.Graphics.SpriteScreen;
        }

        public override void DoUpdate(GameTime gameTime)
        {
            foreach (var player in this.playerMumbleService.OtherPlayerInformation)
            {
                if (!this.playerMarkers.TryGetValue(player.Key, out var marker))
                {
                    marker = new PlayerMarker();
                    GameService.Graphics.World.AddEntity(marker);
                    this.playerMarkers[player.Key] = marker;
                }

                marker.Position = player.Value.MapPosition.ToVector();
            }
            base.DoUpdate(gameTime);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            foreach (var item in this.playerMumbleService.OtherPlayerInformation)
            {
                var location = MapMarkerContainer.MumbleUtils.ContinentToMapScreen(item.Value.ContinentPosition.ToVector());
                location = new Vector2(location.X - 20, location.Y - 10);

                spriteBatch.Draw(this.Texture, new Rectangle((int)location.X, (int)location.Y, 40, 20), Color.White);
            }
        }
    }
}
