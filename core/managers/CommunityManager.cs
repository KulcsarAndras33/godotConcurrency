using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class CommunityManager : Node
{
    private Random random = new();
    private HashSet<IAgent> activeAgents = [];
    private HashSet<IAgent> idleAgents = [];
    private HashSet<IAgent> newIdleAgents = [];
    private List<ICommunityTask> taskQueue = [];

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

    private bool TrySetActionForAgent(IAgent agent)
    {
        var applicableTask = GetFirstNonFullTask(agent);
        if (applicableTask == null)
        {
            return false;
        }

        agent.SetAction(applicableTask.GetAction(agent));
        return true;
    }

    public void AddAgent(IAgent agent)
    {
        agent.communityManager = this;
        activeAgents.Add(agent);
        GD.Print($"Agent added: {activeAgents.Count + idleAgents.Count}");
    }

    public override void _Process(double delta)
    {
        foreach (var agent in activeAgents)
        {
            agent.Tick();
        }

        lock (newIdleAgents)
        {
            foreach (var agent in newIdleAgents)
            {
                GD.Print("Setting agent to idle");
                activeAgents.Remove(agent);
                idleAgents.Add(agent);
            }
            newIdleAgents.Clear();
        }
    }

    public AgentAction AskAgentAction(IAgent agent)
    {
        if (taskQueue.Count == 0)
        {
            lock (newIdleAgents)
            {
                newIdleAgents.Add(agent);
            }
            return new IdleAction();
        }

        var applicableTask = GetFirstNonFullTask(agent);
        if (applicableTask != null)
        {
            return applicableTask.GetAction(agent);
        }

        lock (newIdleAgents)
        {
            newIdleAgents.Add(agent);
        }
        return new IdleAction();
    }

    public void AddTask(ICommunityTask task)
    {
        GD.Print("Adding task");
        GD.Print($"active agents: {activeAgents.Count}, idle agents: {idleAgents.Count}");
        taskQueue.Add(task);
        List<IAgent> newActiveAgents = [];
        foreach (var agent in idleAgents)
        {
            if (TrySetActionForAgent(agent))
            {
                newActiveAgents.Add(agent);
            }
        }

        foreach (var agent in newActiveAgents)
        {
            idleAgents.Remove(agent);
            activeAgents.Add(agent);
        }
        
        GD.Print($"active agents: {activeAgents.Count}, idle agents: {idleAgents.Count}");
    }
 }