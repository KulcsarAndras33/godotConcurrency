public interface ICommunityTask
{
    public bool IsApplicable(IAgent agent);
    public AgentAction GetAction(IAgent agent);
    public bool NeedsMoreAgent();
    public bool IsCompleted();
}