using System;
using System.Collections.Generic;
using System.Linq;
using ChunkSystem.Observation;
using Core.Logic;
using Godot;

public partial class CommunityManager : Node
{
    static readonly int UNLOAD_CHECK_RADIUS = 100;
    static readonly int UNLOAD_THREADPOOL_PRIORITY = 75; // Lowest will be done soonest
    static readonly int STABLE_STATE_THRESHOLD = 2; // In minutes

    private int id = 1; // Default value is TEMPORARY and for testing purposes
    private int objectIdCounter = 1;
    private readonly object idLock = new();
    private Random random = new();
    private HashSet<IAgent> activeAgents = [];
    private HashSet<IAgent> agents = [];
    private readonly HashSet<Building> buildings = [];
    private List<ICommunityTask> taskQueue = [];
    private bool taskRedistributionNeeded = false;
    private readonly TaskDistributor taskDistributor;
    private int finishedVicinityChecks = 0;
    private int neededVicinityChecks = 0;
    private readonly object vicinityLock = new();
    private double timeSinceLastFoodConsumption = 0;

    public readonly ResourceTaskHandler resourceTaskHandler = new();
    public readonly Storage storage = new(1000);


    private void StartVicinityCheck()
    {
        finishedVicinityChecks = 0;


        var chunkManager = ChunkManager.GetInstance();
        GD.Print("Starting vicinity check");
        // Check vicinity of agents for other stuff
        lock (agents)
        {
            neededVicinityChecks = agents.Count;
            foreach (var agent in agents)
            {
                chunkManager.threadPool.Enqueue(() =>
                {
                    lock (vicinityLock)
                    {
                        if (finishedVicinityChecks > neededVicinityChecks)
                        {
                            // Other thread already found something in the vicinity
                            return;
                        }
                    }

                    ObservationBase obs = new SphereObservation(agent.GetPosition(), UNLOAD_CHECK_RADIUS)
                        .OnlyAgent()
                        .SetAgentFilter((a) => a.communityManager != this);

                    ObservationResult result = obs.Observe(chunkManager);

                    if (result.Agents.Count > 0)
                    {
                        lock (vicinityLock)
                        {
                            // Stopping other checks with this special value
                            // Also means that the community CAN NOT BE UNLOADED
                            finishedVicinityChecks = neededVicinityChecks + 1;
                        }
                    }

                    lock (vicinityLock)
                    {
                        finishedVicinityChecks++;
                    }

                }, UNLOAD_THREADPOOL_PRIORITY);
            }
        }
    }

    private void CalculateStableStateAndSave()
    {
        // Calculate time in stable state
        taskDistributor.Distribute([]);
        var resourceUsage = resourceTaskHandler.GetTotalResourceUsageById();

        float totalProduction = 0;
        float minimalExhaustionTime = -1;

        foreach (var entry in resourceUsage)
        {
            GD.Print($"Item {entry.Key} change is {entry.Value}");
            if (entry.Value < 0)
            {
                totalProduction += -entry.Value;
            }
            else if (entry.Value > 0)
            {
                float amount = storage.GetResourceAmount(entry.Key);
                float exhaustionTime = amount / entry.Value;
                if (minimalExhaustionTime == -1 || exhaustionTime < minimalExhaustionTime)
                {
                    minimalExhaustionTime = exhaustionTime;
                }
            }
        }

        float fillTime = storage.GetFreeSpace() / totalProduction;

        GD.Print($"Min exh. time: {minimalExhaustionTime}");
        GD.Print($"Fill time: {fillTime}");

        float stableTime = minimalExhaustionTime == -1 ? fillTime : Math.Min(minimalExhaustionTime, fillTime);

        // If time is enough, save all related chunks
        if (stableTime > STABLE_STATE_THRESHOLD)
        {
            SaveUsedChunks();

            // TODO Add wake-up timer
            //      Save resource change
        }
    }

    private void SaveUsedChunks()
    {
        GD.Print("Saving used chunks");
        HashSet<Chunk> usedChunks = [];

        foreach (var agent in agents)
        {
            Chunk chunk = agent.GetChunk();
            usedChunks.Add(chunk);
        }

        foreach (var chunk in usedChunks)
        {
            chunk.Save();
        }

        lock (agents)
        {
            agents.Clear();
        }
        lock (activeAgents)
        {
            activeAgents.Clear();
        }
        buildings.Clear();

        GD.Print("Saving and clearing finished");

        GC.Collect();
    }

    public CommunityManager()
    {
        resourceTaskHandler.SetCommunity(this);
        taskDistributor = new(this);

        // By default, add some Food
        storage.TryStore(0, 50);
    }

    public void AddAgent(IAgent agent)
    {
        lock (idLock)
        {
            agent.Id = objectIdCounter++;
        }
        agent.communityManager = this;
        activeAgents.Add(agent);
        lock (agents)
        {
            agents.Add(agent);
        }
        GD.Print($"Agent added: {agents.Count}");
    }

    public void AddBuilding(Building building)
    {
        lock (idLock)
        {
            building.Id = objectIdCounter++;
        }

        buildings.Add(building);
    }

    public void BuildingBuilt(Building building)
    {
        resourceTaskHandler.AddBuildings([building]);
    }

    public override void _Process(double delta)
    {
        timeSinceLastFoodConsumption += delta;
        if (timeSinceLastFoodConsumption > 2)
        {
            storage.TryRetrieve("Food", resourceTaskHandler.FoodConsumption / 60 * (float)timeSinceLastFoodConsumption);
            timeSinceLastFoodConsumption = 0;
        }

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

        if (neededVicinityChecks != 0 && finishedVicinityChecks == neededVicinityChecks)
        {
            // Continuing unloading process
            GD.Print("No other agent found in the vicinity.");
            neededVicinityChecks = 0;
            CalculateStableStateAndSave();

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
        taskQueue = [.. taskQueue.Where(task => {
            if (task.IsCompleted()) {
                task.CompletionAction(this);
                return false;
            }
            return true;
            })];

        taskRedistributionNeeded = true;
    }

    // This method DOES NOT take current task distribution into consideration
    // This is temporary
    public void RedistributeTasks()
    {
        GD.Print("Totally redistributing tasks.");
        taskRedistributionNeeded = false;

        TaskDistribution distribution = taskDistributor.Distribute(taskQueue);
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

    public int GetId()
    {
        return id;
    }

    public void TrySave()
    {
        // TODO Is this commmunity active?
        StartVicinityCheck();
    }
}