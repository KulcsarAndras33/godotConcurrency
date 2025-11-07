using Godot;

public class Building : IGridObject
{
    static PackedScene EXAMPLE_SCENE = GD.Load<PackedScene>("res://examples/utils/Building.tscn");

    private int builtLevel = 0;
    private readonly int maxBuiltLevel = 100;
    private readonly Node3D node; // This could later be migrated into a detailed state if needed.
    private Vector3I position;

    private void AddBuildingToChunkSystem()
    {
        var chunkManager = ChunkManager.GetInstance();
        var chunk = chunkManager.GetChunkByPos(position);

        // This was written when data == 1 was walkable, everything else not walkable.
        chunk.SetData(position, 2);
    }

    public Building(Vector3I position)
    {
        this.position = position;
        node = EXAMPLE_SCENE.Instantiate<Node3D>();
        ChunkManager.GetInstance().GetTree().Root.AddChild(node);
        node.Position = position;
    }

    public void Build(int amount)
    {
        if (builtLevel >= maxBuiltLevel)
        {
            GD.Print("Trying to build an already built building.");
        }

        if (builtLevel < maxBuiltLevel)
        {
            builtLevel += amount;
        }

        if (builtLevel >= maxBuiltLevel)
        {
            GD.Print("building is built.");
            AddBuildingToChunkSystem();
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