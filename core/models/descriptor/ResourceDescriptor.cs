using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace core.models.descriptor
{
    class ResourceDescriptor : IDescriptor
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int Id { get; set; }

        [Required]
        public List<string> Tags { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}