using Blish_HUD;
using Denrage.AdventureModule.Interfaces.Mumble;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Services
{
    public class Gw2MumbleWrapper : IGw2Mumble
    {
        public IPlayerCamera PlayerCamera { get; } = new PlayerCameraWrapper();

        public IUserInterface UserInterface { get; } = new UserInterfaceWrapper();

        public ICurrentMap CurrentMap { get; } = new CurrentMapWrapper();

        public IPlayerCharacter PlayerCharacter { get; } = new PlayerCharacterWrapper();

        public bool IsAvailable => GameService.Gw2Mumble.IsAvailable;

        public class PlayerCameraWrapper : IPlayerCamera
        {
            public Vector3 Forward => GameService.Gw2Mumble.PlayerCamera.Forward;

            public Vector3 Position => GameService.Gw2Mumble.PlayerCamera.Position;

            public Matrix View => GameService.Gw2Mumble.PlayerCamera.View;

            public Matrix Projection => GameService.Gw2Mumble.PlayerCamera.Projection;

            public Matrix PlayerView => GameService.Gw2Mumble.PlayerCamera.PlayerView;
        }

        public class UserInterfaceWrapper : IUserInterface
        {
            public Coordinates2 MapPosition => GameService.Gw2Mumble.UI.MapPosition;

            public Size CompassSize => GameService.Gw2Mumble.UI.CompassSize;

            public bool IsMapOpen => GameService.Gw2Mumble.UI.IsMapOpen;

            public bool IsCompassTopRight => GameService.Gw2Mumble.UI.IsCompassTopRight;

            public Coordinates2 MapCenter => GameService.Gw2Mumble.UI.MapCenter;

            public bool IsCompassRotationEnabled => GameService.Gw2Mumble.UI.IsCompassRotationEnabled;

            public double CompassRotation => GameService.Gw2Mumble.UI.CompassRotation;

            public double MapScale => GameService.Gw2Mumble.UI.MapScale;
        }

        public class CurrentMapWrapper : ICurrentMap
        {
            public int Id => GameService.Gw2Mumble.CurrentMap.Id;
        }

        public class PlayerCharacterWrapper : IPlayerCharacter
        {
            public string Name => GameService.Gw2Mumble.PlayerCharacter.Name;

            public Vector3 Position => GameService.Gw2Mumble.PlayerCharacter.Position;

            public MountType CurrentMount => GameService.Gw2Mumble.PlayerCharacter.CurrentMount;

            public RaceType Race => GameService.Gw2Mumble.PlayerCharacter.Race;
        }
    }
}
