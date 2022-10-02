using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Denrage.AdventureModule.Adventure
{
    public class AdventureParser
    {
        private readonly Dictionary<string, AdventureElement> elements = new Dictionary<string, AdventureElement>();

        public AdventureParser()
        {
            Task.Run(async () => await this.Watcher());
        }

        private async Task Watcher()
        {
            try
            {

                while (true)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));

                    using (var file = new FileStream(@"D:\Repos\AdventureModule\test.yaml", FileMode.Open, FileAccess.Read))
                    using (var streamReader = new StreamReader(file))
                    {
                        var yamlStream = new YamlStream();
                        yamlStream.Load(streamReader);
                        var mapping = yamlStream.Documents[0].RootNode as YamlSequenceNode;

                        foreach (var item in mapping.Cast<YamlMappingNode>().SelectMany(x => x))
                        {
                            var name = (item.Key as YamlScalarNode).Value;
                            var type = this.GetNode<YamlScalarNode>(item.Value, "Type");
                            if (type.Value.Equals(nameof(Cuboid), StringComparison.OrdinalIgnoreCase))
                            {
                                var dimensionNode = this.GetNode<YamlScalarNode>(item.Value, nameof(Cuboid.Dimensions));
                                var positionNode = this.GetNode<YamlScalarNode>(item.Value, nameof(Cuboid.Position));

                                var dimensions = this.GetVector3(dimensionNode);
                                var position = this.GetVector3(positionNode);
                                if (!this.elements.TryGetValue(name, out var element))
                                {
                                    element = new Cuboid();
                                    this.elements.Add(name, element);
                                }

                                var cuboid = element as Cuboid;
                                cuboid.Dimensions = dimensions;
                                cuboid.Position = position;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private T GetNode<T>(YamlNode node, string property)
            where T : YamlNode
        {
            var mappingNode = node as YamlMappingNode;
            return mappingNode[new YamlScalarNode(property)] as T;
        }

        private Vector3 GetVector3(YamlNode node)
        {
            var vectorNode = node as YamlScalarNode;
            var parts = vectorNode.Value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
            return new Vector3(parts[0], parts[1], parts[2]);
        }
    }


}


