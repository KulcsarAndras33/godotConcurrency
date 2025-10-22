using Godot;

public class Building : IGridObject
{
    static PackedScene EXAMPLE_SCENE = GD.Load<PackedScene>("res://examples/utils/Building.tscn");

    private int builtLevel = 0;
    private int maxBuiltLevel = 100;
    private Node3D node; // This could later be migrated into a detailed state if needed.
    private Vector3I position;

    public Building(Vector3I position)
    {
        this.position = position;
        node = EXAMPLE_SCENE.Instantiate<Node3D>();
        ChunkManager.GetInstance().GetTree().Root.AddChild(node);
        node.Position = position;
    }

    public void Build(int amount)
    {
        if (builtLevel < maxBuiltLevel)
        {
            builtLevel += amount;
        }

        if (builtLevel >= maxBuiltLevel)
        {
            GD.Print("Buidling is built.");
        }
    }

    public bool IsBuilt()
    {
        return builtLevel >= maxBuiltLevel;
    }

    public Vector3I GetPosition()
    {
        return position;
    }
}