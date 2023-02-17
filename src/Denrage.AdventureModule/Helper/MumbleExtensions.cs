using Blish_HUD;
using Denrage.AdventureModule.Interfaces.Mumble;
using Denrage.AdventureModule.Libs.Messages.Data;
using Gw2Sharp.Models;

namespace Denrage.AdventureModule.Helper
{
    public static class MumbleExtensions
    {
        public static bool Equal(this Vector3 messageVector, Microsoft.Xna.Framework.Vector3 xnaVector)
            => messageVector.X == xnaVector.X && messageVector.Y == xnaVector.Y && messageVector.Z == xnaVector.Z;

        public static bool Equal(this Vector2 messageVector, Microsoft.Xna.Framework.Vector2 xnaVector)
            => messageVector.X == xnaVector.X && messageVector.Y == xnaVector.Y;

        public static bool Equal(this Vector2 messageVector, Coordinates2 coordinates)
            => messageVector.X == coordinates.X && messageVector.Y == coordinates.Y;

        public static bool Equal(this MumbleInformation mumbleInformation, IGw2Mumble mumbleService)
            => mumbleInformation != null && mumbleInformation.CameraPosition.Equal(mumbleService.PlayerCamera.Position) &&
                mumbleInformation.MapPosition.Equal(mumbleService.PlayerCharacter.Position) &&
                mumbleInformation.ContinentPosition.Equal(mumbleService.UserInterface.MapPosition) &&
                mumbleInformation.MapId == mumbleService.CurrentMap.Id &&
                mumbleInformation.CameraDirection.Equal(mumbleService.PlayerCamera.Forward) &&
                mumbleInformation.CharacterName == mumbleService.PlayerCharacter.Name;

        public static Vector2 ToMessageVector(this Microsoft.Xna.Framework.Vector2 vector)
            => new Vector2()
            {
                X = vector.X,
                Y = vector.Y,
            };

        public static Vector2 ToMessageVector(this Coordinates2 coordinates)
            => new Vector2()
            {
                X = (float)coordinates.X,
                Y = (float)coordinates.Y,
            };

        public static Vector3 ToMessageVector(this Microsoft.Xna.Framework.Vector3 vector)
            => new Vector3()
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z,
            };

        public static Microsoft.Xna.Framework.Vector2 ToVector(this Vector2 vector)
            => new Microsoft.Xna.Framework.Vector2()
            {
                X = vector.X,
                Y = vector.Y,
            };

        public static Microsoft.Xna.Framework.Vector3 ToVector(this Vector3 vector)
            => new Microsoft.Xna.Framework.Vector3()
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z,
            };

        public static MumbleInformation ToInformation(this IGw2Mumble service)
            => new MumbleInformation
            {
                CameraDirection = service.PlayerCamera.Forward.ToMessageVector(),
                CharacterName = service.PlayerCharacter.Name,
                CameraPosition = service.PlayerCamera.Position.ToMessageVector(),
                ContinentPosition = service.UserInterface.MapPosition.ToMessageVector(),
                MapId = service.CurrentMap.Id,
                MapPosition = service.PlayerCharacter.Position.ToMessageVector(),
            };
    }
}
