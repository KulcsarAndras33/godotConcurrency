using System.Collections.Generic;
using System.Linq;
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

        private void CalculateBuildingOutput(List<ResourceUsage> outputs)
        {
            foreach (var output in outputs)
            {
                int resourceId = output.ResourceId.Value;
                if (!resourceUsageById.ContainsKey(resourceId))
                {
                    resourceUsageById[resourceId] = 0;
                }
                resourceUsageById[resourceId] -= output.Amount;

                var resourceDescriptor = Library<ResourceDescriptor>.GetInstance().GetDescriptorById(resourceId);
                var resourceTags = resourceDescriptor.Tags;

                // Sub-optimal solution for now
                bool didAssign = false;
                foreach (var tag in resourceTags)
                {
                    if (resourceUsageByTag.GetValueOrDefault(tag, 0) > 0)
                    {
                        resourceUsageByTag[tag] -= output.Amount;
                        didAssign = true;
                        break;
                    }
                }
                if (!didAssign)
                {
                    var tag = resourceTags.First();
                    if (!resourceUsageByTag.ContainsKey(tag))
                    {
                        resourceUsageByTag[tag] = 0;
                    }
                    resourceUsageByTag[tag] -= output.Amount;
                }
            }
        }

        private void CalculateBuildingConsumption(List<ResourceUsage> inputs)
        {
            foreach (var input in inputs)
            {
                // For now, we will only handle input defined by tags
                if (input.ResourceId != null)
                {
                    return;
                }

                var tag = input.ResourceTag;
                if (!resourceUsageByTag.ContainsKey(tag))
                {
                    resourceUsageByTag[tag] = 0;
                }
                resourceUsageByTag[tag] += input.Amount;
            }
        }

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

            // TODO WARNING We will never use a building that has an output resource that's not being used up
            foreach (var usage in resourceUsageByTag)
            {
                GD.Print($"Resource: {usage.Key}, usage: {usage.Value}");
                var currentBuilding = buildingsByResourceTag.GetAvailableBuilding(usage.Key, buildingUsage);
                if (currentBuilding != null)
                {
                    buildingToUse = currentBuilding;
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

            var descriptor = buildingToUse.GetDescriptor();

            CalculateBuildingOutput(descriptor.Outputs);
            CalculateBuildingConsumption(descriptor.Inputs);

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
