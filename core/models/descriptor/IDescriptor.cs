using System.ComponentModel.DataAnnotations;

namespace core.models.descriptor
{
    public interface IDescriptor
    {
        [Required]
        public int Id { get; set; }
    }
}