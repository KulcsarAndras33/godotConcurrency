using System.Collections.Generic;
using core.models.descriptor;
using Core.Model;
using Godot;

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
        private int buildingCapacity = 0;
        private int usedCapacity = 0;

        private Building buildingToUse = null;

        public void AddBuildings(IEnumerable<Building> buildings)
        {
            foreach (var building in buildings)
            {
                if (building.IsBuilt())
                {
                    foreach (var resource in building.GetDescriptor().Outputs)
                    {
                        var resId = resource.ResourceId.Value;
                        buildingsByResourceId.Add(resId, building);
                        foreach (var tag in Library<ResourceDescriptor>.GetInstance().GetDescriptorById(resId).Tags)
                        {
                            buildingsByResourceTag.Add(tag, building);
                        }
                    }

                    buildingCapacity += building.GetDescriptor().MaxWorkers;
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
            usedCapacity = 0;

            resourceUsageByTag["Food"] = 0;

            foreach (var agent in community.GetAgents())
            {
                resourceUsageByTag["Food"] += agent.GetDescriptor().FoodConsumption;
            }

            GD.Print($"Current food usage: {resourceUsageByTag["Food"]}");
        }

        public float GetPriority()
        {
            // This is just a simple way of determining prio
            //      Prios 10 and 2 are chosen by the Stomak method :)
            foreach (var usage in resourceUsageByTag)
            {
                GD.Print("Going through res usages.");
                var currentBuilding = buildingsByResourceTag.GetAvailableBuilding(usage.Key, buildingUsage);
                if (currentBuilding != null)
                {
                    buildingToUse = currentBuilding;
                    GD.Print($"Set building to use to {buildingToUse}");
                    if (usage.Value > 0)
                    {
                        return 10;
                    }
                }
            }

            return 2;
        }

        public ICommunityTask GetTask(TaskDistribution distribution)
        {
            // TODO This takes int, but workforce is float
            if (!buildingUsage.ContainsKey(buildingToUse))
            {
                buildingUsage[buildingToUse] = 0;
            }
            buildingUsage[buildingToUse] += 1;
            usedCapacity += 1;

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

        public float GetResourceUsage(string tag)
        {
            return resourceUsageByTag.GetValueOrDefault(tag);
        }

        public bool HasFreeCapacity()
        {
            GD.Print($"Capacity: Total: {buildingCapacity}, Used: {usedCapacity}");
            return usedCapacity < buildingCapacity;
        }
    }
}
