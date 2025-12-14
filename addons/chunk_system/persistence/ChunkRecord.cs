using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        public static ChunkRecord FromChunk(Vector3I coords, int[,,] data)
        {
            ChunkRecord record = new()
            {
                X = coords.X,
                Y = coords.Y,
                Z = coords.Z,
            };

            var flatData = data.Cast<int>().ToArray();

            using (MemoryStream m = new())
            {
                using BinaryWriter writer = new(m);
                foreach (var value in flatData)
                {
                    writer.Write(value);
                }
                writer.Flush();
                record.Data = m.ToArray();
            }

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