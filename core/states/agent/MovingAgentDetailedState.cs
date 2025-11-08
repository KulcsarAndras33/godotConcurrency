using Godot;

public class MovingAgentDetailedState : IMovingState
{
    static PackedScene EXAMPLE_SCENE = GD.Load<PackedScene>("res://examples/utils/Agent.tscn");

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

    public void Enter()
    {
        node.GlobalPosition = position;
    }

    public void Exit()
    {
        node?.QueueFree();
        node = null;
    }

    public void Load()
    {
        GD.Print("Loading resources for MovingAgentDetailedState");
    }

    public void SetPostion(Vector3 position)
    {
        var chunkManager = ChunkManager.GetInstance();
        var newChunk = chunkManager.GetChunkByPos((Vector3I)position);
        if (agent.CurrentChunk != newChunk)
        {
            agent.CurrentChunk?.RemoveAgent(agent);
            newChunk.AddAgent(agent);
            agent.CurrentChunk = newChunk;
        }

        this.position = position;
        node.GlobalPosition = position;
    }

    public Vector3 GetPostion()
    {
        return position;
    }

    public void Unload()
    {
        GD.Print("Unloading resources for MovingAgentDetailedState");
    }

    public bool IsDetailed()
    {
        return true;
    }

    public bool IsValid()
    {
        return agent.CurrentChunk?.IsDetailed ?? true;
    }

    public IState GetNextState()
    {
        Exit();

        var nextState = new MovingAgentAbstractState(agent, position);
        nextState.Load();
        nextState.Enter();
        return nextState;
    }

    public void NoActionLeft()
    {
        agent.communityManager.NotifyNoAction();
    }

}