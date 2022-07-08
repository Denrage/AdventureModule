using Denrage.AdventureModule.Libs.Messages;
using Denrage.AdventureModule.Libs.Messages.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.Services
{
    public class DrawObjectService
    {
        private ConcurrentDictionary<Type, ConcurrentDictionary<Guid, DrawObject>> drawObjects = new ConcurrentDictionary<Type, ConcurrentDictionary<Guid, DrawObject>>();

        private Dictionary<Type,(
            Func<IEnumerable<object>, Message> AddMessage,
            Func<IEnumerable<Guid>, Message> RemoveMessage,
            Func<IEnumerable<object>, Message> UpdateMessage,
            Action<object, object> UpdateObject)> drawObjectTypeToMessageType = new Dictionary<Type, (
                Func<IEnumerable<object>, Message> AddMessage, 
                Func<IEnumerable<Guid>, Message> RemoveMessage, 
                Func<IEnumerable<object>, Message> UpdateMessage,
                Action<object, object> UpdateObject)>();

        private Dictionary<Type, Dictionary<Guid, DrawObject>> objectsToUpdate = new Dictionary<Type, Dictionary<Guid, DrawObject>>();
        private readonly TcpService tcpService;

        public DrawObjectService(TcpService tcpService)
        {
            this.tcpService = tcpService;
            Task.Run(async () =>
            {
                await this.UpdateTask(default);
            });
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

        public async Task Add<T>(IEnumerable<T> drawObjects, bool fromServer, CancellationToken ct)
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

                if (!fromServer)
                {
                    await this.tcpService.Send(this.drawObjectTypeToMessageType[typeof(T)].AddMessage(drawObjects), ct);
                }
            }

        }

        public async Task Remove<T>(IEnumerable<Guid> drawObject, bool fromServer, CancellationToken ct)
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

                if (!fromServer)
                {
                    await this.tcpService.Send(this.drawObjectTypeToMessageType[typeof(T)].RemoveMessage(drawObject), ct);
                }
            }
        }

        public async Task Update<T>(IEnumerable<T> drawObjects, bool fromServer, CancellationToken ct)
            where T : DrawObject
        {
            if (drawObjects is null || !drawObjects.Any())
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

                if (!fromServer)
                {
                    var type = drawObjects.First().GetType();
                    lock (this.objectsToUpdate)
                    {
                        foreach (var item in drawObjects)
                        {
                            if (!this.objectsToUpdate.TryGetValue(type, out var guidList))
                            {
                                guidList = new Dictionary<Guid, DrawObject>();
                                this.objectsToUpdate[type] = guidList;
                            }

                            guidList[item.Id] = item;
                        }
                    }
                }
            }
        }

        private async Task UpdateTask(CancellationToken ct)
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(20, ct);

                if (!this.objectsToUpdate.Any())
                {
                    continue;
                }

                var objectsToUpdate = new Dictionary<Type, List<DrawObject>>();
                lock (this.objectsToUpdate)
                {
                    foreach (var item in this.objectsToUpdate)
                    {
                        objectsToUpdate.Add(item.Key, new List<DrawObject>(item.Value.Select(x => x.Value)));
                    }

                    this.objectsToUpdate.Clear();
                }

                foreach (var item in objectsToUpdate)
                {
                    await this.tcpService.Send(this.drawObjectTypeToMessageType[item.Key].UpdateMessage(item.Value), ct);
                }

            }
        }

        public IEnumerable<TDrawObject> GetDrawObjects<TDrawObject>()
            where TDrawObject : DrawObject
            => this.drawObjects[typeof(TDrawObject)].Select(x => x.Value).Cast<TDrawObject>();
    }
}
