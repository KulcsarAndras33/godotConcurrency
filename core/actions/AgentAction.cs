using Godot;

public abstract class AgentAction
{
    private ulong nextStepTime = 0;

    protected abstract ulong GetDetailedTimeout();
    protected abstract ulong GetAbstractTimeout();

    protected abstract void DetailedNextStep();
    protected abstract void AbstractNextStep();
    protected void ResetNextStepTime()
    {
        nextStepTime = 0;
    }

    public void DoDetailedStep()
    {
        nextStepTime = Time.GetTicksMsec() + GetDetailedTimeout();
        DetailedNextStep();
    }

    public void DoAbstractStep()
    {
        nextStepTime = Time.GetTicksMsec() + GetAbstractTimeout();
        AbstractNextStep();
    }

    public abstract bool IsComplete();
    public bool IsOnCoolDown()
    {
        return nextStepTime > Time.GetTicksMsec();
    }

    public virtual void HandleStateChange()
    { }
}