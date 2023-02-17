using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gw2MumbleEmulator;
[StructLayout(LayoutKind.Explicit)]
internal unsafe struct Gw2LinkedMem
{
    public const int SIZE = 5460;

    public Gw2LinkedMem(uint version, uint tick, float[] avatarPosition, float[] avatarFront, string name, float[] cameraPosition, float[] cameraFront, string identity, Gw2Context context)
    {
        this.uiVersion = version;
        this.uiTick = tick;
        
        for (int i = 0; i < avatarPosition.Length; i++)
        {
            this.fAvatarPosition[i] = avatarPosition[i];
        }

        for (int i = 0; i < avatarFront.Length; i++)
        {
            this.fAvatarFront[i] = avatarFront[i];
        }

        for (int i = 0; i < name.Length; i++)
        {
            this.name[i] = name[i];
        }

        for (int i = 0; i < cameraPosition.Length; i++)
        {
            this.fCameraPosition[i] = cameraPosition[i];
        }

        for (int i = 0; i < cameraFront.Length; i++)
        {
            this.fCameraFront[i] = cameraFront[i];
        }

        for (int i = 0; i < identity.Length; i++)
        {
            this.identity[i] = identity[i];
        }

        this.contextLen = 256;

        this.context = context;

    }

    [FieldOffset(0)] public uint uiVersion;
    [FieldOffset(4)] public uint uiTick;
    [FieldOffset(8)] public fixed float fAvatarPosition[3];
    [FieldOffset(20)] public fixed float fAvatarFront[3];
    [FieldOffset(44)] public fixed char name[256];
    [FieldOffset(556)] public fixed float fCameraPosition[3];
    [FieldOffset(568)] public fixed float fCameraFront[3];
    [FieldOffset(592)] public fixed char identity[256];
    [FieldOffset(1104)] public uint contextLen;
    [FieldOffset(1108)] public Gw2Context context;

    // Unused fields
#if FALSE
        [FieldOffset(32)]
        public fixed float fAvatarTop[3];
        [FieldOffset(580)]
        public fixed float fCameraTop[3];
        [FieldOffset(1364)]
        public fixed char description[2048];
#endif

    // Total struct size is 5460 bytes
}

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct Gw2Context
{
    public const int SOCKET_ADDRESS_SIZE = 28;

    public Gw2Context(ushort socketAddressFamily, ushort socketPort, byte[] socketAddress4, ushort[] socketAddress6, uint mapId, uint mapType, uint shardId, uint instance, uint buildId, UiState uiState, ushort compassWidth, ushort compassHeight, float compassRotation, float playerMapX, float playerMapY, float mapCenterX, float mapCenterY, float mapScale, uint processId, MountType mount)
    {
        this._socketAddressFamily = socketAddressFamily;
        this.socketPort = socketPort;

        for (int i = 0; i < socketAddress4.Length; i++)
        {
            this.socketAddress4[i] = socketAddress[i];
        }

        for (int i = 0; i < socketAddress6.Length; i++)
        {
            this.socketAddress6[i] = socketAddress6[i];
        }

        this.mapId = mapId;
        this.mapType = mapType;
        this.shardId = shardId;
        this.instance = instance;
        this.buildId = buildId;
        this.uiState = uiState;
        this.compassWidth = compassWidth;
        this.compassHeight = compassHeight;
        this.compassRotation = compassRotation;
        this.playerMapX = playerMapX;
        this.playerMapY = playerMapY;
        this.mapCenterX = mapCenterX;
        this.mapCenterY = mapCenterY;
        this.mapScale = mapScale;
        this.processId = processId;
        this.mount = mount;
    }

    [FieldOffset(0)] public fixed byte socketAddress[SOCKET_ADDRESS_SIZE];

    [FieldOffset(0)] private ushort _socketAddressFamily;

    public AddressFamily socketAddressFamily => (AddressFamily)this._socketAddressFamily;

    [FieldOffset(2)] public ushort socketPort;
    [FieldOffset(4)] public fixed byte socketAddress4[4];
    [FieldOffset(8)] public fixed ushort socketAddress6[8];

    [FieldOffset(28)] public uint mapId;
    [FieldOffset(32)] public uint mapType;
    [FieldOffset(36)] public uint shardId;
    [FieldOffset(40)] public uint instance;
    [FieldOffset(44)] public uint buildId;
    [FieldOffset(48)] public UiState uiState;
    [FieldOffset(52)] public ushort compassWidth;
    [FieldOffset(54)] public ushort compassHeight;
    [FieldOffset(56)] public float compassRotation;
    [FieldOffset(60)] public float playerMapX;
    [FieldOffset(64)] public float playerMapY;
    [FieldOffset(68)] public float mapCenterX;
    [FieldOffset(72)] public float mapCenterY;
    [FieldOffset(76)] public float mapScale;
    [FieldOffset(80)] public uint processId;
    [FieldOffset(84)] public MountType mount;

    // Total struct size is 256 bytes
}

/// <summary>
/// The UI state.
/// </summary>
[Flags]
internal enum UiState : uint
{
    /// <summary>
    /// Whether the map is currently open.
    /// </summary>
    IsMapOpen = 1 << 0,

    /// <summary>
    /// Whether the compass is currently positioned in the top right corner.
    /// </summary>
    IsCompassTopRight = 1 << 1,

    /// <summary>
    /// Whether the compass has its rotation currently enabled.
    /// </summary>
    IsCompassRotationEnabled = 1 << 2,

    /// <summary>
    /// Whether the game window currently has focus.
    /// </summary>
    DoesGameHaveFocus = 1 << 3,

    /// <summary>
    /// Whether the map the player is currently in, is a competitive mode map.
    /// </summary>
    IsCompetitiveMode = 1 << 4,

    /// <summary>
    /// Whether any textbox input field has focus.
    /// </summary>
    DoesAnyInputHaveFocus = 1 << 5,

    /// <summary>
    /// Whether the player is currently in combat.
    /// </summary>
    IsInCombat = 1 << 6
}

/// <summary>
/// Represents a mount.
/// Used by Mumble Link.
/// </summary>
public enum MountType : byte
{
    /// <summary>
    /// No mount.
    /// </summary>
    None,

    /// <summary>
    /// The jackal mount.
    /// </summary>
    Jackal,

    /// <summary>
    /// The griffon mount.
    /// </summary>
    Griffon,

    /// <summary>
    /// The springer mount.
    /// </summary>
    Springer,

    /// <summary>
    /// The skimmer mount.
    /// </summary>
    Skimmer,

    /// <summary>
    /// The raptor mount.
    /// </summary>
    Raptor,

    /// <summary>
    /// The roller beetle mount.
    /// </summary>
    RollerBeetle,

    /// <summary>
    /// The warclaw mount.
    /// </summary>
    Warclaw,

    /// <summary>
    /// The skyscale mount.
    /// </summary>
    Skyscale,

    /// <summary>
    /// The skiff.
    /// </summary>
    Skiff,

    /// <summary>
    /// The siege turtle mount.
    /// </summary>
    SiegeTurtle
}