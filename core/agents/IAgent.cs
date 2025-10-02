public interface IAgent : IGridObject
{
    public CommunityManager communityManager { get; set; }

    void Tick();
    void SetAction(AgentAction action);
}