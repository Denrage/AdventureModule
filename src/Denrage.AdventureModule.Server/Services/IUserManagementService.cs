namespace Denrage.AdventureModule.Server.Services;

public interface IUserManagementService
{
    IEnumerable<Group> Groups { get; }

    event Action<Guid> LoggedIn;

    void AddUser(Guid id, string name);
    Guid GetConnectionIdFromUser(User user);
    Group? GetGroup(User user);
    User? GetUserFromConnectionId(Guid id);
    void UpdateUserId(Guid id, string name);
    bool UserExists(string name);
}