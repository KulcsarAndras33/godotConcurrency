using System.Collections.Generic;

public abstract class BaseCommunityTask : ICommunityTask
{
    protected int agentsNeeded;
    protected HashSet<IAgent> currentAgents = new();

    protected abstract AgentAction CreateAction(IAgent agent);

    public BaseCommunityTask(int agentsNeeded)
    {
        this.agentsNeeded = agentsNeeded;
    }

    public AgentAction GetAction(IAgent agent)
    {
        currentAgents.Add(agent);
        return CreateAction(agent);
    }

    public abstract bool IsApplicable(IAgent agent);

    public abstract bool IsCompleted();

    public bool NeedsMoreAgent()
    {
        return currentAgents.Count < agentsNeeded;
    }

    public virtual float GetPriority()
    {
        return 5;
    }
}