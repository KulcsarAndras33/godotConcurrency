public class BuildAction : AgentAction
{
    private readonly int BUILD_AMOUNT = 5;
    private readonly ulong BUILD_TIMEOUT = 1000;

    public Building building;

    private void Build()
    {
        // TODO Here we can access the properties of the agent to determine building speed.
        building.Build(BUILD_AMOUNT);
    }

    public override bool IsComplete()
    {
        return building.IsBuilt();
    }

    protected override void AbstractNextStep()
    {
        Build();
    }

    protected override void DetailedNextStep()
    {
        Build();
    }

    protected override ulong GetAbstractTimeout()
    {
        return BUILD_TIMEOUT;
    }

    protected override ulong GetDetailedTimeout()
    {
        return BUILD_TIMEOUT;
    }
}