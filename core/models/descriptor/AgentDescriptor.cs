using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace core.models.descriptor
{
    public class AgentDescriptor : IDescriptor
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int Id { get; set; }

        [Required]
        public float FoodConsumption { get; set; }
        public Dictionary<string, object> Properties { private get; set; }

        public override string ToString()
        {
            return Name;
        }

        public T GetProperty<T>(string prop)
        {
            if (!Properties.TryGetValue(prop, out object val))
            {
                return default;
            }

            if (prop is not T)
            {
                return default;
            }

            return (T)val;
        }
    }
}