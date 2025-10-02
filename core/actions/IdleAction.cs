public class IdleAction : AgentAction
{
    public override bool IsComplete()
    {
        return false;
    }

    protected override void AbstractNextStep()
    {
        
    }

    protected override void DetailedNextStep()
    {
        
    }

    protected override ulong GetAbstractTimeout()
    {
        return ulong.MaxValue;
    }

    protected override ulong GetDetailedTimeout()
    {
        return ulong.MaxValue;
    }
}