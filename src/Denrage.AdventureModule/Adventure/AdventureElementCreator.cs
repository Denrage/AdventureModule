using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AdventureModule.Adventure
{
    public class AdventureElementCreator
    {
        private Dictionary<string, AdventureElement> elements = new Dictionary<string, AdventureElement>();

        public IEnumerable<AdventureElement> Elements
        {
            get
            {
                lock (this.elements)
                {
                    return this.elements.Values.ToArray();
                }
            }
        }

        public object CreateCuboid(string name, Vector3 position, Vector3 dimension)
        {
            var result = new Cuboid()
            {
                Position = position,
                Dimensions = dimension,
            };

            this.elements[name] = result;
            return result;
        }

        public object CreateMarker(string name, Vector3 position, Vector3 rotation)
        {
            lock (this.elements)
            {
                var result = new MarkerElement(position, rotation);
                this.elements[name] = result;
                return result;
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
        }
    }


}


