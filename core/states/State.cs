using System;

public abstract class State : IState
{
    private SingleTaskExecutor taskExecutor = new(ChunkManager.GetInstance().threadPool);

    protected abstract void CreateDefaultAction(Action<AgentAction> actionSetter);

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Load();
    public abstract void Unload();
    public void GetDefaultAction(Action<AgentAction> actionSetter)
    {
        taskExecutor.TryExecute(() =>
        {
            CreateDefaultAction(actionSetter);
        });
    }

    public abstract bool IsDetailed();
    public abstract bool IsValid();
    public abstract IState GetNextState();
}