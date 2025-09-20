using System;

public class SingleTaskExecutor
{
    private bool isExecuting = false;
    private PriorityThreadPool threadPool;

    public SingleTaskExecutor(PriorityThreadPool threadPool)
    {
        this.threadPool = threadPool;
    }

    public bool TryExecute(Action action, int priority = PriorityThreadPool.DEFAULT_PRIORTY)
    {
        if (isExecuting)
        {
            return false;
        }

        isExecuting = true;
        threadPool.Enqueue(() =>
        {
            action.Invoke();
            isExecuting = false;
        }, priority);

        return true;
    }

    public bool IsExecuting()
    {
        return isExecuting;
    }
}