using System;

public interface IState
{
    public abstract void Enter();
    public abstract void Exit();
    public abstract void Load();
    public abstract void Unload();
    public void GetDefaultAction(Action<AgentAction> actionSetter);
    public bool IsDetailed();
    public bool IsValid();
    public IState GetNextState();
}