using System.Collections.Generic;
using Godot;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

namespace core.models.descriptor
{
    class Library<T> where T : IDescriptor
    {
        private readonly List<T> descriptors = [];
        private readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNodeDeserializer(inner => new ValidatingNodeDeserializer(inner), s => s.InsteadOf<ObjectNodeDeserializer>())
        .Build();

        private T ParseDescriptor(string filePath)
        {
            return deserializer.Deserialize<T>(new StreamReader(filePath));
        }

        public void ParseDescriptors(string path)
        {
            var files = Directory.GetFiles(path);
            foreach (var filePath in files)
            {
                T descriptor = ParseDescriptor(filePath);
                descriptors.Insert(descriptor.Id, descriptor);
            }

            foreach (var desc in descriptors)
            {
                GD.Print(desc);
            }
        }
    }
}