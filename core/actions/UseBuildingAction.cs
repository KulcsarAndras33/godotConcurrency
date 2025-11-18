public class UseBuildingAction : AgentAction
{
    private readonly ulong USE_TIMEOUT = 1000;

    public Building building;

    private void UseBuilding()
    {
        // TODO Actually do something
    }

    public override bool IsComplete()
    {
        return building.IsBuilt();
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