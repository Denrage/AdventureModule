using Denrage.AdventureModule.Libs.Messages.Data;

namespace Denrage.AdventureModule.Server.Services;

public interface IPlayerMumbleService
{
    void UpdateInformation(string name, MumbleInformation information);
}