using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace core.models.descriptor
{
    class ResourceUsage : IValidatableObject
    {
        public int? ResourceId { get; set; }
        public string ResourceTag { get; set; }

        [Required]
        public int Amount { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ResourceId == null && ResourceTag == null)
            {
                yield return new ValidationResult("Exactly one of 'ResourceId' or 'ResourceTag' must be provided.");
            }
        }
    }
}