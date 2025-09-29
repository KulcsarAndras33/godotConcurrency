
using System;
using Godot;

public class MovingAgentAbstractState : State, IMovingState
{
    private Chunk currChunk;

    public MovingAgent agent;
    public Vector3 position;

    public MovingAgentAbstractState(MovingAgent agent, Vector3 position)
    {
        this.agent = agent;
        this.position = position;
    }

    public override void Enter()
    {
        GD.Print("Entering MovingAgentAbstractState");
    }

    public override void Exit()
    {
    }

    public override void Load()
    {
        GD.Print("Loading resources for MovingAgentAbstractState");
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

    public override void Unload()
    {
        GD.Print("Unloading resources for MovingAgentAbstractState");
    }

    protected override void CreateDefaultAction(Action<AgentAction> actionSetter)
    {
        var goal = agent.communityManager.GetRandomGoal();
        var newAction = new MoveAction
        {
            abstractPath = ChunkManager.GetInstance().FindAbstractPath((Vector3I) position, goal),
            agent = agent
        };
        actionSetter.Invoke(newAction);
    }

    public override bool IsDetailed()
    {
        return false;
    }

    public override bool IsValid()
    {
        return !currChunk?.isDetailed ?? true;
    }

    public override IState GetNextState()
    {
        var nextState = new MovingAgentDetailedState(agent, position);
        nextState.Enter();
        return nextState;
    }
}