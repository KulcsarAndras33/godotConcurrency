using System.Collections.Generic;
using core.models.descriptor;
using Godot;

public interface IAgent : IGridObject
{
    public CommunityManager communityManager { get; set; }

    void Tick();
    void SetActions(List<AgentAction> action);
    AgentDescriptor GetDescriptor();
    Vector3 GetPosition();
    Chunk GetChunk();
}