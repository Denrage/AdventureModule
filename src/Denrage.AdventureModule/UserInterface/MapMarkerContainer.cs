using Blish_HUD;
using Blish_HUD.Controls;
using Denrage.AdventureModule.Helper;
using Denrage.AdventureModule.Interfaces.Mumble;
using Denrage.AdventureModule.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;

namespace Denrage.AdventureModule.UserInterface
{
    public class MapMarkerContainer : Control
    {

        private const int MAPWIDTH_MAX = 362;
        private const int MAPHEIGHT_MAX = 338;
        private const int MAPWIDTH_MIN = 170;
        private const int MAPHEIGHT_MIN = 170;
        private const int MAPOFFSET_MIN = 19;

        private readonly DrawObjectService drawObjectService;
        private readonly IGw2Mumble gw2Mumble;

        public Texture2D Texture { get; set; }

        public MapMarkerContainer(DrawObjectService drawObjectService, IGw2Mumble gw2Mumble)
        {
            this.Texture = Module.Instance.ContentsManager.GetTexture("marker.png");
            this.ClipsBounds = false;
            this.drawObjectService = drawObjectService;
            this.gw2Mumble = gw2Mumble;
            this.Parent = GameService.Graphics.SpriteScreen;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var markers = this.drawObjectService.GetDrawObjects<Libs.Messages.Data.MapMarker>();
            var miniMapRectangle = new Rectangle(0, 0, 0, 0);
            if (!GameService.Gw2Mumble.UI.IsMapOpen)
            {
                var offsetWidth = this.GetOffset(GameService.Gw2Mumble.UI.CompassSize.Width, MAPWIDTH_MAX, MAPWIDTH_MIN, 40);
                var offsetHeight = this.GetOffset(GameService.Gw2Mumble.UI.CompassSize.Height, MAPHEIGHT_MAX, MAPHEIGHT_MIN, 40);

                miniMapRectangle.Location = GameService.Gw2Mumble.UI.IsCompassTopRight
                    ? new Point(GameService.Graphics.SpriteScreen.ContentRegion.Width - GameService.Gw2Mumble.UI.CompassSize.Width - offsetWidth + 1, 1)
                    : new Point(GameService.Graphics.SpriteScreen.ContentRegion.Width - GameService.Gw2Mumble.UI.CompassSize.Width - offsetWidth,
                                              GameService.Graphics.SpriteScreen.ContentRegion.Height - GameService.Gw2Mumble.UI.CompassSize.Height - offsetHeight - 40);

                miniMapRectangle.Size = new Point(GameService.Gw2Mumble.UI.CompassSize.Width + offsetWidth,
                                    GameService.Gw2Mumble.UI.CompassSize.Height + offsetHeight);
            }

            foreach (var item in markers)
            {
                var location = MapUtils.ContinentToMapScreen(item.Position.ToVector(), this.gw2Mumble);
                location = new Vector2(location.X - 20, location.Y - 10);

                if (GameService.Gw2Mumble.UI.IsMapOpen || miniMapRectangle.Contains(location))
                {
                    spriteBatch.Draw(this.Texture, new Rectangle((int)location.X, (int)location.Y, 40, 20), Color.White);
                }
            }
        }

        private int GetOffset(float curr, float max, float min, float val)
            => (int)Math.Round((curr - min) / (max - min) * (val - MAPOFFSET_MIN) + MAPOFFSET_MIN, 0);

    }
}
