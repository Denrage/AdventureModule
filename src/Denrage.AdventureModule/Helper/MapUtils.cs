using Blish_HUD;
using Microsoft.Xna.Framework;
using System;

namespace Denrage.AdventureModule.UserInterface
{
    public static class MapUtils
    {
        private const int MinCompassWidth = 170;
        private const int MaxCompassWidth = 362;
        private const int MinCompassHeight = 170;
        private const int MaxCompassHeight = 338;
        private const int MinCompassOffset = 19;
        private const int MaxCompassOffset = 40;
        private const int CompassSeparation = 40;

        private static int GetCompassOffset(float curr, float min, float max)
            => (int)Math.Round(MathUtils.Scale(curr, min, max, MinCompassOffset, MaxCompassOffset));

        public static Rectangle GetMapBounds()
        {
            if (GameService.Gw2Mumble.UI.CompassSize.Width < 1 || GameService.Gw2Mumble.UI.CompassSize.Height < 1)
            {
                return default;
            }

            if (GameService.Gw2Mumble.UI.IsMapOpen)
            {
                return new Rectangle(Point.Zero, GameService.Graphics.SpriteScreen.Size);
            }

            var offsetWidth = GetCompassOffset(GameService.Gw2Mumble.UI.CompassSize.Width, MinCompassWidth, MaxCompassWidth);
            var offsetHeight = GetCompassOffset(GameService.Gw2Mumble.UI.CompassSize.Height, MinCompassHeight, MaxCompassHeight);

            return new Rectangle(
                GameService.Graphics.SpriteScreen.Width - GameService.Gw2Mumble.UI.CompassSize.Width - offsetWidth,
                GameService.Gw2Mumble.UI.IsCompassTopRight
                    ? 0
                    : GameService.Graphics.SpriteScreen.Height - GameService.Gw2Mumble.UI.CompassSize.Height - offsetHeight - CompassSeparation,
                GameService.Gw2Mumble.UI.CompassSize.Width + offsetWidth,
                GameService.Gw2Mumble.UI.CompassSize.Height + offsetHeight);
        }

        public static Vector2 ContinentToMapScreen(Vector2 continentCoords)
        {
            var mapCenter = new Vector2((float)GameService.Gw2Mumble.UI.MapCenter.X, (float)GameService.Gw2Mumble.UI.MapCenter.Y);
            var mapRotation = Matrix.CreateRotationZ(
                GameService.Gw2Mumble.UI.IsCompassRotationEnabled && !GameService.Gw2Mumble.UI.IsMapOpen
                    ? (float)GameService.Gw2Mumble.UI.CompassRotation
                    : 0);

            var screenBounds = GetMapBounds();
            var scale = (float)(GameService.Gw2Mumble.UI.MapScale * 0.897);  // Magic number to transform scale
            var boundsCenter = screenBounds.Location.ToVector2() + screenBounds.Size.ToVector2() / 2f;

            return Vector2.Transform((continentCoords - mapCenter) / scale, mapRotation) + boundsCenter;
        }

        public static Vector2 ScreenToContinentCoords(Vector2 screenCoordinates)
        {
            var mapCenter = GameService.Gw2Mumble.UI.MapCenter;
            var scale = (float)GameService.Gw2Mumble.UI.MapScale * 0.897f;
            var boundsCenter = GameService.Graphics.SpriteScreen.Size.ToVector2() / 2f;
            return ((screenCoordinates - boundsCenter) * scale) + new Vector2((float)mapCenter.X, (float)mapCenter.Y);
        }
    }
}
