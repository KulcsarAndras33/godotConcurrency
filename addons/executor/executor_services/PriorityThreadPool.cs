using System;
using System.Collections.Generic;
using System.Threading;

public class PriorityThreadPool : IDisposable
{
    private readonly PriorityQueue<Action, int> _taskQueue = new();
    private readonly List<Thread> _workers;
    private readonly object _lock = new();
    private readonly ManualResetEvent _taskAvailable = new(false);
    private bool _isRunning = true;

    public PriorityThreadPool(int workerCount)
    {
        _workers = new List<Thread>(workerCount);
        for (int i = 0; i < workerCount; i++)
        {
            Thread thread = new Thread(WorkerLoop)
            {
                IsBackground = true
            };
            thread.Start();
            _workers.Add(thread);
        }
    }

    public void Enqueue(Action task, int priority = 50)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        lock (_lock)
        {
            _taskQueue.Enqueue(new Action(task), priority);
            _taskAvailable.Set(); // Signal that a task is available
        }
    }

    private void WorkerLoop()
    {
        while (_isRunning)
        {
            Action task = null;

            while (_taskQueue.Count == 0 && _isRunning)
            {
                _taskAvailable.Reset();
            }

            lock (_lock)
            {
                if (!_isRunning) break;

                if (_taskQueue.Count > 0)
                {
                    task = _taskQueue.Dequeue();
                }
            }

            task?.Invoke();
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _isRunning = false;
            Monitor.PulseAll(_lock); // Wake up all workers to exit
        }

        foreach (var thread in _workers)
        {
            thread.Join();
        }
    }
}
