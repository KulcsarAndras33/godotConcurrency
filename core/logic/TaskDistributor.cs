using System;
using System.Collections.Generic;
using System.Linq;

public class TaskDistributor
{
    static float WORKFORCE_UNIT = 1;

    // TODO This will get the buildings as well
    public static TaskDistribution Distribute(List<ICommunityTask> playerTasks, float workforce)
    {
        float workforceWeight = 0.5f / workforce;

        float remainingWorkforce = workforce;
        TaskDistribution result = new();
        while (remainingWorkforce > 0.1 && playerTasks.Count > 0)
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
    
    private static ICommunityTask GetHighestPrioTask(List<ICommunityTask> tasks, TaskDistribution currentDistribution, float workforceWeight)
    {
        return tasks.MaxBy(task => {
            var prio = task.GetPriority();

            // Lowering priority based on how much workforce is already assigned
            if (currentDistribution.TasksWithWorkforce.TryGetValue(task, out float workforce))
            {
                prio -= workforce * workforceWeight;
            }

            return prio;
        });
    }
}