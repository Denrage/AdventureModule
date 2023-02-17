// See https://aka.ms/new-console-template for more information
using Gw2MumbleEmulator;
using System.IO.MemoryMappedFiles;
using System.Net.Sockets;

Console.WriteLine("Hello, World!");

var mockMumble = System.Text.Json.JsonSerializer.Deserialize<MockMumble>(File.ReadAllText(@"D:\Repos\Blish-HUD\Blish HUD\bin\x64\Debug\net472\mumble.json"));
var uiState = (UiState)0;
if (mockMumble.IsMapOpen)
{
    uiState |= UiState.IsMapOpen;
}

if (mockMumble.IsCompassTopRight)
{
    uiState |= UiState.IsCompassTopRight;
}

if (mockMumble.IsCompassRotationEnabled)
{
    uiState |= UiState.IsCompassRotationEnabled;
}

if (mockMumble.DoesGameHaveFocus)
{
    uiState |= UiState.DoesGameHaveFocus;
}

if (mockMumble.IsCompetitiveMode)
{
    uiState |= UiState.IsCompetitiveMode;
}

if (mockMumble.DoesAnyInputHaveFocus)
{
    uiState |= UiState.DoesAnyInputHaveFocus;
}

if (mockMumble.IsInCombat)
{
    uiState |= UiState.IsInCombat;
}

var memory = new Gw2LinkedMem((uint)mockMumble.Version, (uint)mockMumble.Tick, mockMumble.AvatarPosition, mockMumble.AvatarFront, mockMumble.Name, mockMumble.CameraPosition, mockMumble.CameraFront, mockMumble.RawIdentity, new Gw2Context((ushort)AddressFamily.InterNetwork, (ushort)mockMumble.ServerPort, mockMumble.ServerAddress.Split(".").Select(x => byte.Parse(x)).ToArray(), Array.Empty<ushort>(), (uint)mockMumble.MapId, (uint)mockMumble.MapType, (uint)mockMumble.ShardId, (uint)mockMumble.Instance, (uint)mockMumble.BuildId, uiState, (ushort)mockMumble.Compass[0], (ushort)mockMumble.Compass[1], mockMumble.CompassRotation, mockMumble.PlayerLocationMap[0], mockMumble.PlayerLocationMap[1], mockMumble.MapCenter[0], mockMumble.MapCenter[1], mockMumble.MapScale, (uint)mockMumble.ProcessId, (MountType)mockMumble.Mount));

var mappedFile = MemoryMappedFile.CreateOrOpen("MumbleLink", Gw2LinkedMem.SIZE, MemoryMappedFileAccess.ReadWrite);
var accessor = mappedFile.CreateViewAccessor();

Task.Run(() => WriteMumbleFile(mappedFile, accessor, ref memory));

var quit = false;

while (!quit)
{
    var key = Console.ReadKey();
    switch (key.Key)
    {
        case ConsoleKey.Q:
            quit = true;
            break;
        default:
            break;
    }
}

accessor.Dispose();
mappedFile.Dispose();


void WriteMumbleFile(MemoryMappedFile mappedFile, MemoryMappedViewAccessor accessor, ref Gw2LinkedMem mem)
{
    while (true)
    {
        accessor.Write(0, ref memory);
        Thread.Sleep(10);
        memory.uiTick += 1;
    }
}