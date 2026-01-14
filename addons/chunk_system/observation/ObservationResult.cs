using System.Collections.Generic;

namespace ChunkSystem.Observation
{
    public class ObservationResult
    {
        public List<IAgent> Agents { get; set; } = [];
        public List<Building> Buildings { get; set; } = [];
    }
}