using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO;
using System.Linq;
using Godot;

namespace ChunkSystem.Persistence
{
    [Table("Chunks")]
    public class ChunkRecord
    {
        [Key]
        public int X { get; set; }
        [Key]
        public int Y { get; set; }
        [Key]
        public int Z { get; set; }
        public byte[] Data { get; set; }
        public byte[] Buildings { get; set; }
        public byte[] Agents { get; set; }

        private static byte[] IntArrayToBlob(int[] array)
        {
            using MemoryStream m = new();
            using BinaryWriter writer = new(m);

            foreach (var value in array)
            {
                writer.Write(value);
            }

            writer.Flush();
            return m.ToArray();
        }

        private static byte[] GetBuildingBlob(IEnumerable<Building> buildings)
        {
            // Times 2 since they have a Community Id and an Id
            int[] references = new int[buildings.Count() * 2];

            int i = 0;
            foreach (var building in buildings)
            {
                references[i++] = building.communityManager.GetId();
                references[i++] = building.Id;
            }

            return IntArrayToBlob(references);
        }

        private static byte[] GetAgentBlob(List<IAgent> agents)
        {
            // Times 2 since they have a Community Id and an Id
            int[] references = new int[agents.Count * 2];

            int i = 0;
            foreach (var agent in agents)
            {
                references[i++] = agent.communityManager.GetId();
                references[i++] = agent.Id;
            }

            return IntArrayToBlob(references);
        }

        public static ChunkRecord FromChunk(Vector3I coords, int[,,] data, IEnumerable<Building> buildings, List<IAgent> agents)
        {
            ChunkRecord record = new()
            {
                X = coords.X,
                Y = coords.Y,
                Z = coords.Z,
            };

            var flatData = data.Cast<int>().ToArray();

            record.Data = IntArrayToBlob(flatData);
            record.Buildings = GetBuildingBlob(buildings);
            record.Agents = GetAgentBlob(agents);

            return record;
        }

        public Vector3I GetCoords()
        {
            return new Vector3I(X, Y, Z);
        }

        public int[,,] GetData(Vector3I size)
        {
            using MemoryStream ms = new(Data);
            using BinaryReader reader = new(ms);

            int[,,] data = new int[size.X, size.Y, size.Z];

            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    for (int z = 0; z < size.Z; z++)
                    {
                        data[x, y, z] = reader.ReadInt32();
                    }
                }
            }

            return data;
        }
    }
}