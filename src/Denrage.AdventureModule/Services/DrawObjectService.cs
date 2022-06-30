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
        private Dictionary<Type, (Func<IEnumerable<object>, Message> AddMessage, Func<IEnumerable<Guid>, Message> RemoveMessage)> drawObjectTypeToMessageType = new Dictionary<Type, (Func<IEnumerable<object>, Message> AddMessage, Func<IEnumerable<Guid>, Message> RemoveMessage)>();
        private readonly TcpService tcpService;

        public DrawObjectService(TcpService tcpService)
        {
            this.tcpService = tcpService;
        }

        public void Register<T, TAddMessage, TRemoveMessage>(Func<IEnumerable<T>, TAddMessage> addMessage, Func<IEnumerable<Guid>, TRemoveMessage> removeMessage)
            where T : DrawObject
            where TAddMessage : AddDrawObjectMessage<T>
            where TRemoveMessage : RemoveDrawObjectMessage<T>
        {
            this.drawObjects.TryAdd(typeof(T), new ConcurrentDictionary<Guid, DrawObject>());
            this.drawObjectTypeToMessageType.Add(typeof(T), (items => addMessage(items.Cast<T>()), ids => removeMessage(ids)));
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

        public IEnumerable<TDrawObject> GetDrawObjects<TDrawObject>()
            where TDrawObject : DrawObject
            => this.drawObjects[typeof(TDrawObject)].Select(x => x.Value).Cast<TDrawObject>();
    }
}
