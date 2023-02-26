using Denrage.AdventureModule.Adventure.Elements;
using Denrage.AdventureModule.Interfaces.Mumble;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AdventureModule.Adventure.Services
{
    public class AdventureElementCreator
    {
        private readonly Dictionary<Step, Dictionary<string, AdventureElement>> localElements = new Dictionary<Step, Dictionary<string, AdventureElement>>();
        private readonly Dictionary<string, AdventureElement> elements = new Dictionary<string, AdventureElement>();
        private readonly AdventureDebugService debugService;
        private readonly IGw2Mumble gw2Mumble;

        public AdventureElementCreator(AdventureDebugService debugService, IGw2Mumble gw2Mumble)
        {
            this.debugService = debugService;
            this.gw2Mumble = gw2Mumble;
        }

        public IEnumerable<AdventureElement> Elements
        {
            get
            {
                lock (this.elements)
                {
                    return this.elements.Values.ToArray().Concat(this.localElements.SelectMany(x => x.Value).Select(x => x.Value).ToArray());
                }
            }
        }

        public object CreateCuboid(string name, Vector3 position, Vector3 dimension, int mapId, Step step, bool global)
        {
            var result = new Cuboid(mapId, this.debugService, this.gw2Mumble)
            {
                Position = position,
                Dimensions = dimension,
            };

            if (global)
            {
                lock (this.elements)
                {
                    this.elements[name] = result;
                }
            }
            else
            {
                lock (this.localElements)
                {
                    if (!this.localElements.TryGetValue(step, out var elementDict))
                    {
                        elementDict = new Dictionary<string, AdventureElement>();
                        this.localElements[step] = elementDict;
                    }

                    elementDict[name] = result;
                }
            }

            return result;
        }

        public object CreateMarker(string name, string textureName, Vector3 position, Vector3 rotation, int mapId, Step step, bool global, float fadeNear = -1, float fadeFar = -1)
        {
            var result = new MarkerElement(position, rotation, mapId, textureName, this.debugService, this.gw2Mumble, fadeNear, fadeFar);

            if (global)
            {
                lock (this.elements)
                {
                    this.elements[name] = result;
                }
            }
            else
            {
                lock (this.localElements)
                {
                    if (!this.localElements.TryGetValue(step, out var elementDict))
                    {
                        elementDict = new Dictionary<string, AdventureElement>();
                        this.localElements[step] = elementDict;
                    }

                    elementDict[name] = result;
                }
            }

            return result;
        }

        public void ClearFromStep(Step step)
        {
            lock (this.localElements)
            {
                if (this.localElements.TryGetValue(step, out var elementDict))
                {
                    foreach (var item in elementDict)
                    {
                        item.Value.Dispose();
                    }

                    elementDict.Clear();
                    _ = this.localElements.Remove(step);
                }
            }
        }

        public void Clear()
        {
            lock (this.elements)
            {
                var elements = this.Elements;
                this.elements.Clear();
                foreach (var item in elements)
                {
                    item.Dispose();
                }
            }

            lock (this.localElements)
            {
                foreach (var item in this.localElements)
                {
                    foreach (var innerItem in item.Value)
                    {
                        innerItem.Value.Dispose();
                    }

                    item.Value.Clear();
                }

                this.localElements.Clear();
            }
        }
    }
}
