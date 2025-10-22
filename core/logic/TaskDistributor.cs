using System;
using System.Collections.Generic;
using System.Linq;

public class TaskDistributor
{
    static float WORKFORCE_UNIT = 1;


    // TODO This will get the buildings as well
    public static TaskDistribution Distribute(List<ICommunityTask> playerTasks, float workforce)
    {
        float remainingWorkforce = workforce;
        TaskDistribution result = new();
        while (remainingWorkforce > 0.1)
        {
            float currentWorkforce = Math.Min(remainingWorkforce, WORKFORCE_UNIT);
            ICommunityTask currentTask = GetHighestPrioTask(playerTasks);
            if (!result.TasksWithWorkforce.ContainsKey(currentTask))
            {
                result.TasksWithWorkforce.Add(currentTask, 0);
            }

            result.TasksWithWorkforce[currentTask] += currentWorkforce;
        }

        return result;
    }
    
    private static ICommunityTask GetHighestPrioTask(List<ICommunityTask> tasks)
    {
        // TODO Do other more complicated priority calculations
        return tasks.MaxBy(task => task.GetPriority());
    }
}