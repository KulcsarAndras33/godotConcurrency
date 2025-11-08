using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace core.models.descriptor
{
    class BuildingDescriptor : IDescriptor
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int Id { get; set; }

        [Required]
        public List<ResourceUsage> Inputs { get; set; }

        [Required]
        public List<ResourceUsage> Outputs { get; set; }

        [Required]
        public int MaxWorkers { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}