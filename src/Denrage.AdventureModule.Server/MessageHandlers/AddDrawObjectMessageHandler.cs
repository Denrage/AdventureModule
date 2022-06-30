using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using Denrage.AdventureModule.Libs.Messages.Handler;
using Denrage.AdventureModule.Server.Services;
using System.Collections.Concurrent;

namespace Denrage.AdventureModule.Server.MessageHandlers;

public class AddDrawObjectMessageHandler<TDrawObject> : MessageHandler<AddDrawObjectMessage<TDrawObject>>
    where TDrawObject : DrawObject
{
    private readonly Func<DrawObjectService> getDrawObjectService;

    public AddDrawObjectMessageHandler(Func<DrawObjectService> getDrawObjectService)
    {
        this.getDrawObjectService = getDrawObjectService;
    }

    protected override async Task Handle(Guid clientId, AddDrawObjectMessage<TDrawObject> message, CancellationToken ct)
    {
        Console.WriteLine("Draw Object Count: " + (message.DrawObjects?.Length ?? 0));
        await this.getDrawObjectService().Add(message.DrawObjects, clientId, ct);
    }
}

public class RemoveDrawObjectMessageHandler<TDrawObject> : MessageHandler<RemoveDrawObjectMessage<TDrawObject>>
    where TDrawObject : DrawObject
{
    private readonly Func<DrawObjectService> getDrawObjectService;

    public RemoveDrawObjectMessageHandler(Func<DrawObjectService> getDrawObjectService)
    {
        this.getDrawObjectService = getDrawObjectService;
    }

    protected override async Task Handle(Guid clientId, RemoveDrawObjectMessage<TDrawObject> message, CancellationToken ct)
    {
        await this.getDrawObjectService().Remove<TDrawObject>(message.Ids, clientId, ct);
    }
}

public class DrawObjectService
{
    private ConcurrentDictionary<Type, ConcurrentDictionary<Guid, DrawObject>> drawObjects = new();
    private Dictionary<Type, (Func<IEnumerable<object>, Message> AddMessage, Func<IEnumerable<Guid>, Message> RemoveMessage)> drawObjectTypeToMessageType = new();
    private readonly TcpService tcpService;

    public DrawObjectService(TcpService tcpService, UserManagementService userManagementService)
    {
        this.tcpService = tcpService;
        userManagementService.LoggedIn += id =>
        {
            var user = userManagementService.GetUserFromConnectionId(id);
            foreach (var item in this.drawObjects)
            {
                var addMessage = this.drawObjectTypeToMessageType[item.Key];

                _ = this.tcpService.SendMessage(id, addMessage.AddMessage(item.Value.Values.Cast<object>()), default);
            }
        };
    }

    public void Register<T, TAddMessage, TRemoveMessage>(Func<IEnumerable<T>, TAddMessage> addMessage, Func<IEnumerable<Guid>, TRemoveMessage> removeMessage)
        where T : DrawObject
        where TAddMessage : AddDrawObjectMessage<T>
        where TRemoveMessage : RemoveDrawObjectMessage<T>
    {
        this.drawObjects.TryAdd(typeof(T), new ConcurrentDictionary<Guid, DrawObject>());
        this.drawObjectTypeToMessageType.Add(typeof(T), (items => addMessage(items.Cast<T>()), ids => removeMessage(ids)));
    }

    public async Task Add<T>(IEnumerable<T> drawObjects, Guid clientId, CancellationToken ct)
        where T : DrawObject
    {
        if (drawObjects is null)
        {
            return;
        }

        if (this.drawObjects.TryGetValue(typeof(T), out var objects))
        {
            foreach (var item in drawObjects)
            {
                objects.TryAdd(item.Id, item);
            }

            await this.tcpService.SendToGroup(
                clientId,
                new AddDrawObjectMessage<T>()
                {
                    DrawObjects = drawObjects.ToArray(),
                },
                ct);
        }

    }

    public async Task Remove<T>(IEnumerable<Guid> drawObject, Guid clientId, CancellationToken ct)
        where T : DrawObject
    {
        if (drawObject is null)
        {
            return;
        }

        if (this.drawObjects.TryGetValue(typeof(T), out var objects))
        {
            foreach (var item in drawObject)
            {
                objects.TryRemove(item, out _);
            }

            await this.tcpService.SendToGroup(
                clientId,
                new RemoveDrawObjectMessage<T>()
                {
                    Ids = drawObject.ToArray(),
                },
                ct);
        }
    }
}