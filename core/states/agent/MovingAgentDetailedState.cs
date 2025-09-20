
using System;
using Godot;

public class MovingAgentDetailedState : State, IMovingState
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

    public override void Enter()
    {
        GD.Print("Entering MovingAgentDetailedState");
    }

    public override void Exit()
    {
        GD.Print("Exiting MovingAgentDetailedState");
    }

    public override void Load()
    {
        GD.Print("Loading resources for MovingAgentDetailedState");
    }

    public void SetPostion(Vector3 position)
    {
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
        var goal = agent.communityManager.GetRandomGoal();
        var newAction = new MoveAction
        {
            abstractPath = ChunkManager.GetInstance().FindAbstractPath((Vector3I) position, goal),
            agent = agent
        };
        actionSetter.Invoke(newAction);
    }
}