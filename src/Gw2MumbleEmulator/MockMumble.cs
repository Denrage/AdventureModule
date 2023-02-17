using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2MumbleEmulator;

public class MockMumble
{
    public bool IsAvailable { get; set; }
    public int Version { get; set; }
    public int Tick { get; set; }
    public string Name { get; set; }
    public float[] AvatarPosition { get; set; }
    public float[] AvatarFront { get; set; }
    public float[] CameraPosition { get; set; }
    public float[] CameraFront { get; set; }
    public string ServerAddress { get; set; }
    public int ServerPort { get; set; }
    public int BuildId { get; set; }
    public bool IsMapOpen { get; set; }
    public bool IsCompassTopRight { get; set; }
    public bool IsCompassRotationEnabled { get; set; }
    public bool DoesGameHaveFocus { get; set; }
    public bool IsCompetitiveMode { get; set; }
    public bool DoesAnyInputHaveFocus { get; set; }
    public bool IsInCombat { get; set; }
    public int[] Compass { get; set; }
    public float CompassRotation { get; set; }
    public float[] PlayerLocationMap { get; set; }
    public float[] MapCenter { get; set; }
    public float MapScale { get; set; }
    public int ProcessId { get; set; }
    public int Mount { get; set; }
    public int MapId { get; set; }
    public int MapType { get; set; }
    public int ShardId { get; set; }
    public int Instance { get; set; }
    public string RawIdentity { get; set; }
    public string CharacterName { get; set; }
    public int Profession { get; set; }
    public int Specialization { get; set; }
    public int Race { get; set; }
    public int TeamColorId { get; set; }
    public bool IsCommander { get; set; }
    public float FieldOfView { get; set; }
    public int UiSize { get; set; }
}
