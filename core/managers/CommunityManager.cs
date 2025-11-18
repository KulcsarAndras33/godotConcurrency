using System;
using System.Collections.Generic;
using System.Linq;
using Core.Logic;
using Godot;

public partial class CommunityManager : Node
{
    private Random random = new();
    private HashSet<IAgent> activeAgents = [];
    private HashSet<IAgent> agents = [];
    private readonly HashSet<Building> buildings = [];
    private List<ICommunityTask> taskQueue = [];
    private bool taskRedistributionNeeded = false;
    private readonly ResourceTaskHandler resourceTaskHandler = new();

    public readonly Storage storage = new(1000);

    public void AddAgent(IAgent agent)
    {
        agent.communityManager = this;
        activeAgents.Add(agent);
        agents.Add(agent);
        GD.Print($"Agent added: {agents.Count}");
    }

    public void AddBuilding(Building building)
    {
        buildings.Add(building);
    }

    public void BuildingBuilt(Building building)
    {
        resourceTaskHandler.AddBuildings([building]);
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

        if (taskRedistributionNeeded)
        {
            RedistributeTasks();
        }
    }

    public void AddTask(ICommunityTask task)
    {
        GD.Print("Adding task");
        taskQueue.Add(task);
        taskRedistributionNeeded = true;
    }

    public void NotifyNoAction()
    {
        GD.Print("There's an agent without task.");

        // Clear completed tasks
        taskQueue = [.. taskQueue.Where(task => !task.IsCompleted())];

        taskRedistributionNeeded = true;
    }

    // This method DOES NOT take current task distribution into consideration
    // This is temporary
    public void RedistributeTasks()
    {
        GD.Print("Totally redistributing tasks.");
        taskRedistributionNeeded = false;

        TaskDistribution distribution = TaskDistributor.Distribute(taskQueue, this);
        lock (activeAgents)
        {
            activeAgents.Clear();
        }
        var enumerator = agents.GetEnumerator();

        foreach (var task in distribution.TasksWithWorkforce.Keys)
        {
            for (int j = 0; j < distribution.TasksWithWorkforce[task]; j++)
            {
                enumerator.MoveNext();
                var currentAgent = enumerator.Current;
                // TODO Try this with the threadpool
                currentAgent.SetActions(task.GetActions(currentAgent));
                lock (activeAgents)
                {
                    activeAgents.Add(currentAgent);
                }
            }
        }
    }

    public HashSet<IAgent> GetAgents()
    {
        return agents;
    }
}