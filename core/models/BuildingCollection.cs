using System.Collections.Generic;

namespace Core.Model
{
    public class BuildingCollection<T>
    {
        private readonly Dictionary<T, List<Building>> data = [];

        private bool CanBuildingBeUsed(Building building, Dictionary<Building, int> buildingUsage)
        {
            if (!buildingUsage.TryGetValue(building, out int usage))
            {
                return true;
            }

            if (usage < building.GetDescriptor().MaxWorkers)
            {
                return true;
            }

            return false;
        }

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
            if (!data.TryGetValue(key, out var buildings))
            {
                return null;
            }

            foreach (var building in buildings)
            {
                if (CanBuildingBeUsed(building, buildingUsage))
                {
                    return building;
                }
            }

            return null;
        }

        public int GetCount(T key)
        {
            if (!data.TryGetValue(key, out List<Building> value))
            {
                return 0;
            }
            return value.Count;
        }

        public Building GetAvailableBuilding(Dictionary<Building, int> buildingUsage)
        {
            foreach (var buildings in data.Values)
            {
                foreach (var building in buildings)
                {
                    if (CanBuildingBeUsed(building, buildingUsage))
                    {
                        return building;
                    }
                }
            }

            return null;
        }
    }
}