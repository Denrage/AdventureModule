using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Interfaces.Mumble
{
    public interface IGw2Mumble
    {
        IPlayerCamera PlayerCamera { get; }

        IUserInterface UserInterface { get; }

        ICurrentMap CurrentMap { get; }

        IPlayerCharacter PlayerCharacter { get; }

        bool IsAvailable { get; }
    }

    public interface IPlayerCamera
    {
        Vector3 Forward { get; }

        Vector3 Position { get; }

        Matrix View { get; }

        Matrix Projection { get; }

        Matrix PlayerView { get; }
    }

    public interface IUserInterface
    {
        Coordinates2 MapPosition { get; }

        Size CompassSize { get; }

        bool IsMapOpen { get; }

        bool IsCompassTopRight { get; }

        Coordinates2 MapCenter { get; }

        bool IsCompassRotationEnabled { get; }

        double CompassRotation { get; }

        double MapScale { get; }
    }

    public interface ICurrentMap
    {
        int Id { get; }
    }

    public interface IPlayerCharacter
    {
        string Name { get; }

        Vector3 Position { get; }

        MountType CurrentMount { get; }

        RaceType Race { get; }
    }
}
