using Microsoft.Xna.Framework;

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

        public object CreateMarker(string name, string textureName, Vector3 position, Vector3 rotation, int mapId, float fadeNear = -1, float fadeFar = -1)
            => this.creator.CreateMarker(name, textureName, position, rotation, mapId, this.step, false, fadeNear, fadeFar);

        public object CreateMarkerGlobal(string name, string textureName, Vector3 position, Vector3 rotation, int mapId, float fadeNear = -1, float fadeFar = -1)
            => this.creator.CreateMarker(name, textureName, position, rotation, mapId, this.step, true, fadeNear, fadeFar);
    }
}
