using Blish_HUD;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Services
{
    public class MapMarkerService
    {
        public ConcurrentDictionary<Guid, Libs.Messages.Data.MapMarker> UserMarker { get; } = new ConcurrentDictionary<Guid, Libs.Messages.Data.MapMarker>();

        public ConcurrentDictionary<Guid, Libs.Messages.Data.MapMarker> OtherPlayerMarkers { get; } = new ConcurrentDictionary<Guid, Libs.Messages.Data.MapMarker>();

        public void AddUserMarker(IEnumerable<Libs.Messages.Data.MapMarker> markers)
        {
            foreach (var marker in markers)
            {
                this.UserMarker.TryAdd(marker.Id, marker);
            }
        }

        public void DeleteUserMarker(Guid id)
        {
            this.UserMarker.TryRemove(id, out _);
        }

        public Libs.Messages.Data.MapMarker GetUserMarker(Vector2 screenPosition)
        {
            var x = screenPosition.X - 10;
            var y = screenPosition.Y - 10;
            var maxX = screenPosition.X + 10;
            var maxY = screenPosition.Y + 10;

            var topLeft = this.ScreenToContinentCoords(new Vector2(x, y));
            var bottomRight = this.ScreenToContinentCoords(new Vector2(maxX, maxY));

            foreach (var marker in this.UserMarker)
            {
                if (marker.Value.Position.X > topLeft.X && marker.Value.Position.X < bottomRight.X && marker.Value.Position.Y > topLeft.Y && marker.Value.Position.Y < bottomRight.Y)
                {
                    return marker.Value;
                }
            }

            return null;
        }

        public void AddServerMarker(IEnumerable<Libs.Messages.Data.MapMarker> markers)
        {
            foreach (var marker in markers)
            {
                this.OtherPlayerMarkers.TryAdd(marker.Id, marker);
            }
        }

        public void RemoveServerMarker(IEnumerable<Guid> markers)
        {
            foreach (var marker in markers)
            {
                this.OtherPlayerMarkers.TryRemove(marker, out _);
            }
        }

        private Vector2 ScreenToContinentCoords(Vector2 screenCoordinates)
        {
            var mapCenter = GameService.Gw2Mumble.UI.MapCenter;
            var scale = (float)GameService.Gw2Mumble.UI.MapScale * 0.897f;
            var boundsCenter = GameService.Graphics.SpriteScreen.Size.ToVector2() / 2f;
            return ((screenCoordinates - boundsCenter) * scale) + new Vector2((float)mapCenter.X, (float)mapCenter.Y);
        }
    }
}
