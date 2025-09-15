using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;

public class PriorityTaskScheduler : TaskScheduler
{
    public const int DEFAULT_PRIORITY = 50;

    private PriorityQueue<Task, int> taskQueue = new();
    private int delegatesRunning = 0;

    protected override IEnumerable<Task> GetScheduledTasks()
    {
        throw new System.NotImplementedException();
    }

    protected override void QueueTask(Task task)
    {
        taskQueue.Enqueue(task, DEFAULT_PRIORITY);
        if (delegatesRunning < MaximumConcurrencyLevel)
        {
            delegatesRunning++;
            ThreadPool.UnsafeQueueUserWorkItem(_ => {
                while (true)
                {
                    Task item;
                    lock (taskQueue)
                    {
                        if (!taskQueue.TryDequeue(out item, out int prio))
                        {
                            delegatesRunning--;
                            break;
                        }
                    }

                    TryExecuteTask(item);
                }
            }, null);
        }       
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        throw new System.NotImplementedException();
    }
}