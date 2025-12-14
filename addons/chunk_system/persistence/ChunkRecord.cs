using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Godot;

namespace ChunkSystem.Persistence
{
    [Table("Chunks")]
    public class ChunkRecord
    {
        [Key]
        public Vector3I Coordinates { get; set; }
        public int[,,] Data { get; set; }
    }
}