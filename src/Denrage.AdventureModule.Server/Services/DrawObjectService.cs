﻿using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using System.Collections.Concurrent;

namespace Denrage.AdventureModule.Server.Services;

public class SynchronizationService
{
    private readonly TcpService tcpService;
    private readonly UserManagementService userManagementService;
    private readonly ConcurrentDictionary<Guid, IState> currentStates = new ConcurrentDictionary<Guid, IState>();

    public SynchronizationService(TcpService tcpService, UserManagementService userManagementService)
    {
        this.tcpService = tcpService;
        this.userManagementService = userManagementService;
        //userManagementService.LoggedIn += id =>
        //{
        //    var user = userManagementService.GetUserFromConnectionId(id);
        //    foreach (var item in this.currentStates)
        //    {
        //        if (item.Value is DialogState dialogState)
        //        {
        //            _ = this.tcpService.SendMessage(id, new StateChangedMessage<DialogState>() { State = dialogState }, default);
        //        }
        //    }
        //};
    }

    public async Task StateChanged(IState state, Guid clientId, CancellationToken ct)
    {
        _ = this.currentStates.TryAdd(state.Id, state);
        var group = this.userManagementService.GetGroup(this.userManagementService.GetUserFromConnectionId(clientId));

        if (state is DialogState dialogState)
        {
            await this.tcpService.SendToGroup(group, new StateChangedMessage<DialogState>() { State = dialogState }, ct);
        }

        if (state is LuaVariablesState variableState)
        {
            await this.tcpService.SendToGroup(group, new StateChangedMessage<LuaVariablesState>() { State = variableState }, ct);
        }

        if (state is AdventureState adventureState)
        {
            await this.tcpService.SendToGroup(group, new StateChangedMessage<AdventureState>() { State = adventureState }, ct);
        }
    }
}

public class DrawObjectService
{
    private ConcurrentDictionary<Type, ConcurrentDictionary<Guid, DrawObject>> drawObjects = new();
    private Dictionary<Type, (
        Func<IEnumerable<object>, Message> AddMessage,
        Func<IEnumerable<Guid>, Message> RemoveMessage,
        Func<IEnumerable<object>, Message> UpdateMessage,
        Action<object, object> UpdateObject)> drawObjectTypeToMessageType = new();

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

    public void Register<T, TAddMessage, TRemoveMessage, TUpdateMessage>(
        Func<IEnumerable<T>, TAddMessage> addMessage,
        Func<IEnumerable<Guid>, TRemoveMessage> removeMessage,
        Func<IEnumerable<T>, TUpdateMessage> updateMessage,
        Action<T, T> updateObject)
        where T : DrawObject
        where TAddMessage : AddDrawObjectMessage<T>
        where TRemoveMessage : RemoveDrawObjectMessage<T>
        where TUpdateMessage : UpdateDrawObjectMessage<T>
    {
        this.drawObjects.TryAdd(typeof(T), new ConcurrentDictionary<Guid, DrawObject>());
        this.drawObjectTypeToMessageType.Add(typeof(T),
            (items => addMessage(items.Cast<T>()),
            ids => removeMessage(ids),
            items => updateMessage(items.Cast<T>()),
            (oldObject, newObject) => updateObject((T)oldObject, (T)newObject)));
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

    public async Task Update<T>(IEnumerable<T> drawObjects, Guid clientId, CancellationToken ct)
    where T : DrawObject
    {
        if (drawObjects is null)
        {
            return;
        }

        if (this.drawObjects.TryGetValue(typeof(T), out var objects))
        {
            var update = this.drawObjectTypeToMessageType[typeof(T)].UpdateObject;
            foreach (var item in drawObjects)
            {
                update(objects[item.Id], item);
            }

            await this.tcpService.SendToGroup(
                clientId,
                new UpdateDrawObjectMessage<T>()
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