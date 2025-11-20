using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;
using System;

namespace core.models.descriptor
{
    public class Library<T> where T : IDescriptor
    {
        private readonly List<T> descriptors = [];
        private readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNodeDeserializer(inner => new ValidatingNodeDeserializer(inner), s => s.InsteadOf<ObjectNodeDeserializer>())
        .Build();
        private readonly string name = "Default library name";
        private static Library<T> instance = null;

        public static Library<T> GetInstance()
        {
            instance ??= new Library<T>($"{typeof(T).Name} library");

            return instance;
        }

        private Library(string name)
        {
            this.name = name;
        }

        private T ParseDescriptor(string filePath)
        {
            using var reader = new StreamReader(filePath);
            return deserializer.Deserialize<T>(reader);
        }

        public void ParseDescriptors(string path)
        {
            descriptors.Clear();

            var files = Directory.GetFiles(path);
            foreach (var filePath in files)
            {
                T descriptor = ParseDescriptor(filePath);
                descriptors.Insert(descriptor.Id, descriptor);
            }
        }

        public T GetDescriptorById(int id)
        {
            if (id < 0 || id >= descriptors.Count)
            {
                throw new Exception($"Unknown descriptor id: {id} in library {name}");
            }

            return descriptors[id];
        }
    }
}