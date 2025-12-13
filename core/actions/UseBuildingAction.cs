public class UseBuildingAction : AgentAction
{
    private readonly ulong USE_TIMEOUT = 1000;

    public Building building;
    public IAgent agent;

    private void UseBuilding()
    {
        building.Interact(agent, USE_TIMEOUT);
    }

    public override bool IsComplete()
    {
        return false;
    }

    protected override void AbstractNextStep()
    {
        UseBuilding();
    }

    protected override void DetailedNextStep()
    {
        UseBuilding();
    }

    protected override ulong GetAbstractTimeout()
    {
        return USE_TIMEOUT;
    }

    protected override ulong GetDetailedTimeout()
    {
        return USE_TIMEOUT;
    }
}