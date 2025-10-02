
using System;
using Godot;

public class MovingAgentDetailedState : State, IMovingState
{
    static PackedScene EXAMPLE_SCENE = GD.Load<PackedScene>("res://examples/utils/Agent.tscn");

    private Chunk currChunk;

    public MovingAgent agent;
    public Vector3 position;

    public Node3D node;

    public MovingAgentDetailedState(MovingAgent agent, Vector3 position)
    {
        this.agent = agent;
        this.position = position;
        node = EXAMPLE_SCENE.Instantiate<Node3D>();
        ChunkManager.GetInstance().GetTree().Root.AddChild(node);
    }

    public override void Enter()
    {
        node.GlobalPosition = position;
    }

    public override void Exit()
    {
        node.QueueFree();
    }

    public override void Load()
    {
        GD.Print("Loading resources for MovingAgentDetailedState");
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

        if (!newChunk.isDetailed)
        {
            Exit();
        }
        currChunk = newChunk;

        this.position = position;
        node.GlobalPosition = position;
    }

    public Vector3 GetPostion()
    {
        return position;
    }

    public override void Unload()
    {
        GD.Print("Unloading resources for MovingAgentDetailedState");
    }

    protected override void CreateDefaultAction(Action<AgentAction> actionSetter)
    {
        actionSetter.Invoke(agent.communityManager.AskAgentAction(agent));
    }

    public override bool IsDetailed()
    {
        return true;
    }

    public override bool IsValid()
    {
        return currChunk?.isDetailed ?? true;
    }

    public override IState GetNextState()
    {
        var nextState = new MovingAgentAbstractState(agent, position);
        nextState.Load();
        nextState.Enter();
        return nextState;
    }
}