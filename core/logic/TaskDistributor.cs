using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Core.Logic
{

    public class TaskDistributor
    {
        static readonly float WORKFORCE_UNIT = 1;
        private readonly CommunityManager community;
        public TaskDistributor(CommunityManager community)
        {
            this.community = community;
        }

        // TODO This will get the buildings as well
        public TaskDistribution Distribute(List<ICommunityTask> playerTasks)
        {
            float workforce = community.GetAgents().Count;
            float workforceWeight = 0.5f / workforce;
            community.resourceTaskHandler.ResetResourceConsumption();

            float remainingWorkforce = workforce;
            TaskDistribution result = new();
            while (remainingWorkforce > 0.1 && (playerTasks.Count > 0 || community.resourceTaskHandler.HasFreeCapacity()))
            {
                float currentWorkforce = Math.Min(remainingWorkforce, WORKFORCE_UNIT);
                ICommunityTask currentTask = GetHighestPrioTask(playerTasks, result, workforceWeight);
                if (!result.TasksWithWorkforce.ContainsKey(currentTask))
                {
                    result.TasksWithWorkforce.Add(currentTask, 0);
                }

                result.TasksWithWorkforce[currentTask] += currentWorkforce;
                remainingWorkforce -= currentWorkforce;
            }

            return result;
        }

        private ICommunityTask GetHighestPrioTask(List<ICommunityTask> tasks, TaskDistribution currentDistribution, float workforceWeight)
        {
            var highestPrioTask = tasks.MaxBy(task =>
            {
                var prio = task.GetPriority();

                // Lowering priority based on how much workforce is already assigned
                if (currentDistribution.TasksWithWorkforce.TryGetValue(task, out float workforce))
                {
                    prio -= workforce * workforceWeight;
                }

                return prio;
            });

            GD.Print(highestPrioTask);
            var resPrio = community.resourceTaskHandler.GetPriority();

            if (highestPrioTask == null || resPrio > highestPrioTask.GetPriority())
            {
                GD.Print("Getting task from resource thingy.");
                highestPrioTask = community.resourceTaskHandler.GetTask(currentDistribution);
            }

            return highestPrioTask;
        }
    }

}