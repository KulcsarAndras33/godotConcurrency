using System.Collections.Generic;
using Core.Model;

namespace Core.Logic
{
    public class ResourceTaskHandler
    {
        private readonly BuildingCollection<int> buildingsByResourceId = new();
        private readonly BuildingCollection<string> buildingsByResourceTag = new();
        private readonly Dictionary<Building, int> buildingUsage = [];
        private readonly Dictionary<int, float> resourceUsageById = [];
        private readonly Dictionary<string, float> resourceUsageByTag = [];
        private CommunityManager community;

        private Building buildingToUse = null;

        public void AddBuildings(IEnumerable<Building> buildings)
        {
            foreach (var building in buildings)
            {
                if (building.IsBuilt())
                {
                    foreach (var resource in building.GetDescriptor().Outputs)
                    {
                        if (resource.ResourceId != null)
                        {
                            buildingsByResourceId.Add(resource.ResourceId.Value, building);
                        }
                        if (resource.ResourceTag != null)
                        {
                            buildingsByResourceTag.Add(resource.ResourceTag, building);
                        }
                    }
                }
            }
        }

        public void SetCommunity(CommunityManager communityManager)
        {
            community = communityManager;
        }

        public void ResetResourceConsumption()
        {
            resourceUsageById.Clear();
            resourceUsageByTag.Clear();
            buildingUsage.Clear();

            resourceUsageByTag["Food"] = 0;

            foreach (var agent in community.GetAgents())
            {
                resourceUsageByTag["Food"] += agent.GetDescriptor().FoodConsumption;
            }
        }

        public float GetPriority()
        {
            // This is just a simple way of determining prio
            //      Prios 10 and 2 are chosen by the Stomak method :)
            foreach (var usage in resourceUsageByTag)
            {
                buildingToUse = buildingsByResourceTag.GetAvailableBuilding(usage.Key, buildingUsage);
                if (usage.Value > 0 && buildingToUse != null)
                {
                    return 10;
                }
            }

            return 2;
        }

        public ICommunityTask GetTask(TaskDistribution distribution)
        {
            // TODO This takes int, but workforce is float
            buildingUsage[buildingToUse] += 1;

            foreach (var output in buildingToUse.GetDescriptor().Outputs)
            {
                if (output.ResourceId != null)
                {
                    if (!resourceUsageById.ContainsKey(output.ResourceId.Value))
                    {
                        resourceUsageById[output.ResourceId.Value] = 0;
                    }
                    resourceUsageById[output.ResourceId.Value] -= output.Amount;
                }
                if (output.ResourceTag != null)
                {
                    if (!resourceUsageByTag.ContainsKey(output.ResourceTag))
                    {
                        resourceUsageByTag[output.ResourceTag] = 0;
                    }
                    resourceUsageByTag[output.ResourceTag] -= output.Amount;
                }
            }

            foreach (var task in distribution.TasksWithWorkforce)
            {
                if (task.Key is not UseBuildingTask)
                {
                    continue;
                }

                var actualTask = task.Key as UseBuildingTask;
                if (actualTask.building == buildingToUse)
                {
                    return actualTask;
                }
            }

            return new UseBuildingTask(buildingToUse);
        }
    }
}
