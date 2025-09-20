public interface IAgent
{
    public CommunityManager communityManager { get; set; }

    void Tick();
}