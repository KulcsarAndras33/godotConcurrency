using System.ComponentModel.DataAnnotations;

namespace Core.Persistence
{
    public class AgentRecord
    {
        [Key]
        public int CommunityId { get; set; }
        [Key]
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int DescriptorId { get; set; }
    }
}