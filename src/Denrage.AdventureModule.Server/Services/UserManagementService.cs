using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Server.Services;

public class UserManagementService : IUserManagementService
{
    private readonly ConcurrentBag<Group> groups = new();
    private readonly ConcurrentDictionary<Guid, User> users = new();

    public event Action<Guid>? LoggedIn;

    public UserManagementService()
    {
        this.groups.Add(new Group() { Name = "Default" });
    }

    public User? GetUserFromConnectionId(Guid id) 
        => this.users.TryGetValue(id, out var user) ? user : null;

    public Guid GetConnectionIdFromUser(User user) 
        => this.users.FirstOrDefault(x => x.Value == user).Key;

    public void AddUser(Guid id, string name)
    {
        var user = new User() { Name = name };
        _ = this.users.AddOrUpdate(id, user, (id, oldValue) => user);
        this.groups.First().Users.Add(user);
        this.LoggedIn?.Invoke(id);
    }

    public void UpdateUserId(Guid id, string name)
    {
        var key = this.users.Where(x => x.Value.Name == name).FirstOrDefault();

        if (!key.Equals(default))
        {
            _ = this.users.TryRemove(key.Key, out var user);
            _ = this.users.AddOrUpdate(id, user, (id, oldValue) => user);
            this.LoggedIn?.Invoke(id);
        }
    }

    public bool UserExists(string name)
        => this.users.Any(x => x.Value.Name == name);

    public Group? GetGroup(User user) 
        => this.groups.FirstOrDefault(x => x.Users.Contains(user));

    public IEnumerable<Group> Groups
        => this.groups.ToArray();
}

public class User
{
    public string Name { get; set; }
}

public class Group
{
    public string Name { get; set; }

    public ConcurrentBag<User> Users { get; set; } = new();
}
