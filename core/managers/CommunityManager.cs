using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;

public partial class CommunityManager : Node
{
    private Random random = new();
    private HashSet<IAgent> activeAgents = [];
    private HashSet<IAgent> agents = [];
    private List<ICommunityTask> taskQueue = [];
    private Storage storage = new(1000);

    private ICommunityTask GetFirstNonFullTask(IAgent agent)
    {
        foreach (var task in taskQueue)
        {
            if (task.NeedsMoreAgent() && task.IsApplicable(agent))
            {
                return task;
            }
        }

        return null;
    }

    public void AddAgent(IAgent agent)
    {
        agent.communityManager = this;
        activeAgents.Add(agent);
        agents.Add(agent);
        GD.Print($"Agent added: {agents.Count}");
    }

    public override void _Process(double delta)
    {
        lock (activeAgents)
        {
            foreach (var agent in activeAgents)
            {
                agent.Tick();
            }
        }
    }

    public void AddTask(ICommunityTask task)
    {
        GD.Print("Adding task");
        taskQueue.Add(task);
        RedistributeTasks();
    }

    public void NotifyNoAction()
    {
        GD.Print("There's an agent without task.");
        RedistributeTasks();
    }
    
    // This method DOES NOT take current task distribution into consideration
    // This is temporary
    public void RedistributeTasks()
    {
        GD.Print("Totally redistributing tasks.");
        TaskDistribution distribution = TaskDistributor.Distribute(taskQueue, agents.Count);
        lock (activeAgents) {
            activeAgents.Clear();
        }
        var enumerator = agents.GetEnumerator();

        foreach (var task in distribution.TasksWithWorkforce.Keys)
        {
            for (int j = 0; j < distribution.TasksWithWorkforce[task]; j++)
            {
                enumerator.MoveNext();
                var currentAgent = enumerator.Current;
                currentAgent.SetAction(task.GetAction(currentAgent));
                activeAgents.Add(currentAgent);
            }
        }
    }
 }