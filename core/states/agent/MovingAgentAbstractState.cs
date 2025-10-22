
using System;
using Godot;

public class MovingAgentAbstractState : IMovingState
{
    private Chunk currChunk;

    public MovingAgent agent;
    public Vector3 position;

    public MovingAgentAbstractState(MovingAgent agent, Vector3 position)
    {
        this.agent = agent;
        this.position = position;
    }

    public void Enter()
    {
        GD.Print("Entering MovingAgentAbstractState");
    }

    public void Exit()
    {
    }

    public void Load()
    {
        GD.Print("Loading resources for MovingAgentAbstractState");
    }

    public void Unload()
    {
        GD.Print("Unloading resources for MovingAgentAbstractState");
    }

    public void SetPostion(Vector3 position)
    {
        var chunkManager = ChunkManager.GetInstance();
        var newChunk = chunkManager.GetChunkByPos((Vector3I) position);
        if (currChunk != newChunk)
        {
            currChunk?.RemoveAgent(agent);
            newChunk.AddAgent(agent);
        }

        if (newChunk.isDetailed)
        {
            Exit();
        }

        currChunk = newChunk;
        this.position = position;
    }

    public Vector3 GetPostion()
    {
        return position;
    }

    public  bool IsDetailed()
    {
        return false;
    }

    public  bool IsValid()
    {
        return !currChunk?.isDetailed ?? true;
    }

    public  IState GetNextState()
    {
        var nextState = new MovingAgentDetailedState(agent, position);
        nextState.Enter();
        return nextState;
    }

    public void NoActionLeft()
    {
        agent.communityManager.NotifyNoAction();
    }
}