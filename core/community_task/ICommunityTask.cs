using System.Collections.Generic;

public interface ICommunityTask
{
    public bool IsApplicable(IAgent agent);
    public List<AgentAction> GetActions(IAgent agent);
    public bool IsCompleted();
    public void CompletionAction(CommunityManager communityManager);
    public float GetPriority();
    public void AddAgent(IAgent agent);
    public void RemoveAgent(IAgent agent);
    public void ClearAgents();
}