using Blish_HUD;
using Blish_HUD.Controls;
using Denrage.AdventureModule.Helper;
using Denrage.AdventureModule.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Denrage.AdventureModule.UserInterface
{
    public class MapMarkerContainer : Control
    {
        private readonly DrawObjectService drawObjectService;

        public Texture2D Texture { get; set; }

        public MapMarkerContainer(DrawObjectService drawObjectService)
        {
            this.Texture = Module.Instance.ContentsManager.GetTexture("marker.png");
            this.ClipsBounds = false;
            this.drawObjectService = drawObjectService;
            this.Parent = GameService.Graphics.SpriteScreen;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var markers = this.drawObjectService.GetDrawObjects<Libs.Messages.Data.MapMarker>();
            foreach (var item in markers)
            {
                var location = MapUtils.ContinentToMapScreen(item.Position.ToVector());
                location = new Vector2(location.X - 20, location.Y - 10);
                spriteBatch.Draw(this.Texture, new Rectangle((int)location.X, (int)location.Y, 40, 20), Color.White);
            }

        }

    }
}
