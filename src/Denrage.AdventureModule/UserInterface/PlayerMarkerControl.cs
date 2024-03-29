﻿using Blish_HUD;
using Blish_HUD.Controls;
using Denrage.AdventureModule.Entities;
using Denrage.AdventureModule.Helper;
using Denrage.AdventureModule.Interfaces.Mumble;
using Denrage.AdventureModule.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
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
        private readonly IGw2Mumble gw2Mumble;
        private readonly Dictionary<string, PlayerMarker> playerMarkers = new Dictionary<string, PlayerMarker>();
        
        public Texture2D Texture { get; set; }

        public PlayerMarkerControl(PlayerMumbleService playerMumbleService, IGw2Mumble gw2Mumble)
        {
            this.playerMumbleService = playerMumbleService;
            this.gw2Mumble = gw2Mumble;
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
                    marker = new PlayerMarker(this.gw2Mumble);
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
                var location = MapUtils.ContinentToMapScreen(item.Value.ContinentPosition.ToVector(), this.gw2Mumble);
                spriteBatch.DrawString(ContentService.Content.DefaultFont14, item.Value.CharacterName, location, Color.White);
                location = new Vector2(location.X - 20, location.Y - 10);

                spriteBatch.Draw(this.Texture, new Rectangle((int)location.X, (int)location.Y, 40, 20), Color.White);
            }
        }
    }
}
