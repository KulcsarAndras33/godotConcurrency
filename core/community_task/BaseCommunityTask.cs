using System.Collections.Generic;

public abstract class BaseCommunityTask : ICommunityTask
{
    protected HashSet<IAgent> currentAgents = new();

    public abstract bool IsApplicable(IAgent agent);

    public abstract bool IsCompleted();

    public virtual float GetPriority()
    {
        return 5;
    }

    public void AddAgent(IAgent agent)
    {
        currentAgents.Add(agent);
    }
    public void RemoveAgent(IAgent agent)
    {
        currentAgents.Remove(agent);
    }
    public void ClearAgents()
    {
        currentAgents.Clear();
    }

    public abstract List<AgentAction> GetActions(IAgent agent);
}