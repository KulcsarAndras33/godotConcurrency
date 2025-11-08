using System.ComponentModel.DataAnnotations;

namespace core.models.descriptor
{
    interface IDescriptor
    {
        [Required]
        public int Id { get; set; }
    }
}