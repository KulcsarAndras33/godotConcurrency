using System.Collections.Generic;
using Godot;

public partial class PathVisualiser : Node
{
    static PackedScene detailedPathScene = GD.Load<PackedScene>("res://examples/utils/DetailedPathNode.tscn");
    static PackedScene asbtractPathScene = GD.Load<PackedScene>("res://examples/utils/AbstractPathNode.tscn");
    private MovingAgent agent;

    private void DrawPath(List<Vector3I> path, PackedScene scene)
    {
        if (path == null) return;

        foreach (var pos in path)
        {
            var instance = scene.Instantiate<Node3D>();
            AddChild(instance);
            instance.GlobalPosition = pos;
        }
    }

    private void RemoveChildren()
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
    }

    public override void _Process(double delta)
    {
        if (agent == null) return;

        RemoveChildren();

        MoveAction action = agent.GetMoveAction();
        if (action == null)
        {
            return;
        }

        DrawPath(action.detailedPath, detailedPathScene);
        DrawPath(action.abstractPath, asbtractPathScene);
    }

    public void SetAgent(MovingAgent agent)
    {
        this.agent = agent;
    }
}