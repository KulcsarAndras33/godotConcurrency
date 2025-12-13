using System.Collections.Generic;
using core.models.descriptor;

public interface IAgent : IGridObject
{
    public CommunityManager communityManager { get; set; }

    void Tick();
    void SetActions(List<AgentAction> action);
    AgentDescriptor GetDescriptor();
}