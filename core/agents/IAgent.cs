using System.Collections.Generic;

public interface IAgent : IGridObject
{
    public CommunityManager communityManager { get; set; }

    void Tick();
    void SetActions(List<AgentAction> action);
}