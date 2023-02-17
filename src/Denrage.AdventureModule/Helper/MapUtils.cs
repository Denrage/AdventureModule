using Blish_HUD;
using Denrage.AdventureModule.Interfaces.Mumble;
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

        public static Rectangle GetMapBounds(IGw2Mumble gw2Mumble)
        {
            if (gw2Mumble.UserInterface.CompassSize.Width < 1 || gw2Mumble.UserInterface.CompassSize.Height < 1)
            {
                return default;
            }

            if (gw2Mumble.UserInterface.IsMapOpen)
            {
                return new Rectangle(Point.Zero, GameService.Graphics.SpriteScreen.Size);
            }

            var offsetWidth = GetCompassOffset(gw2Mumble.UserInterface.CompassSize.Width, MinCompassWidth, MaxCompassWidth);
            var offsetHeight = GetCompassOffset(gw2Mumble.UserInterface.CompassSize.Height, MinCompassHeight, MaxCompassHeight);

            return new Rectangle(
                GameService.Graphics.SpriteScreen.Width - gw2Mumble.UserInterface.CompassSize.Width - offsetWidth,
                gw2Mumble.UserInterface.IsCompassTopRight
                    ? 0
                    : GameService.Graphics.SpriteScreen.Height - gw2Mumble.UserInterface.CompassSize.Height - offsetHeight - CompassSeparation,
                gw2Mumble.UserInterface.CompassSize.Width + offsetWidth,
                gw2Mumble.UserInterface.CompassSize.Height + offsetHeight);
        }

        public static Vector2 ContinentToMapScreen(Vector2 continentCoords, IGw2Mumble gw2Mumble)
        {
            var mapCenter = new Vector2((float)gw2Mumble.UserInterface.MapCenter.X, (float)gw2Mumble.UserInterface.MapCenter.Y);
            var mapRotation = Matrix.CreateRotationZ(
                gw2Mumble.UserInterface.IsCompassRotationEnabled && !gw2Mumble.UserInterface.IsMapOpen
                    ? (float)gw2Mumble.UserInterface.CompassRotation
                    : 0);

            var screenBounds = GetMapBounds(gw2Mumble);
            var scale = (float)(gw2Mumble.UserInterface.MapScale * 0.897);  // Magic number to transform scale
            var boundsCenter = screenBounds.Location.ToVector2() + screenBounds.Size.ToVector2() / 2f;

            return Vector2.Transform((continentCoords - mapCenter) / scale, mapRotation) + boundsCenter;
        }

        public static Vector2 ScreenToContinentCoords(Vector2 screenCoordinates, IGw2Mumble gw2Mumble)
        {
            var mapCenter = gw2Mumble.UserInterface.MapCenter;
            var scale = (float)gw2Mumble.UserInterface.MapScale * 0.897f;
            var boundsCenter = GameService.Graphics.SpriteScreen.Size.ToVector2() / 2f;
            return ((screenCoordinates - boundsCenter) * scale) + new Vector2((float)mapCenter.X, (float)mapCenter.Y);
        }
    }
}
