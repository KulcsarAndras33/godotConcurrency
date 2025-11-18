using System.Collections.Generic;

namespace Core.Model
{
    public class BuildingCollection<T>
    {
        private readonly Dictionary<T, List<Building>> data = [];

        public void Add(T key, Building building)
        {
            if (!data.TryGetValue(key, out List<Building> value))
            {
                value = [];
                data[key] = value;
            }

            value.Add(building);
        }

        public Building GetAvailableBuilding(T key, Dictionary<Building, int> buildingUsage)
        {
            foreach (var building in data[key])
            {
                if (!buildingUsage.TryGetValue(building, out int usage))
                {
                    return building;
                }
                if (usage < building.GetDescriptor().MaxWorkers)
                {
                    return building;
                }
            }

            return null;
        }
    }
}