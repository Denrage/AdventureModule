using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AdventureModule.Adventure
{
    public class AdventureElementCreatorWrapper
    {
        private readonly Step step;
        private readonly AdventureElementCreator creator;

        public AdventureElementCreatorWrapper(Step step, AdventureElementCreator creator)
        {
            this.step = step;
            this.creator = creator;
        }

        public object CreateCuboid(string name, Vector3 position, Vector3 dimension, int mapId)
            => this.creator.CreateCuboid(name, position, dimension, mapId, this.step, false);

        public object CreateCuboidGlobal(string name, Vector3 position, Vector3 dimension, int mapId)
            => this.creator.CreateCuboid(name, position, dimension, mapId, this.step, true);

        public object CreateMarker(string name, Vector3 position, Vector3 rotation, int mapId)
            => this.creator.CreateMarker(name, position, rotation, mapId, this.step, false);

        public object CreateMarkerGlobal(string name, Vector3 position, Vector3 rotation, int mapId)
            => this.creator.CreateMarker(name, position, rotation, mapId, this.step, true);
    }

    public class AdventureElementCreator
    {
        private Dictionary<Step, Dictionary<string, AdventureElement>> localElements = new Dictionary<Step, Dictionary<string, AdventureElement>>();

        private Dictionary<string, AdventureElement> elements = new Dictionary<string, AdventureElement>();

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
            var result = new Cuboid(mapId)
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

        public object CreateMarker(string name, Vector3 position, Vector3 rotation, int mapId, Step step, bool global)
        {
            var result = new MarkerElement(position, rotation, mapId);
            
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

            lock(this.localElements)
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
