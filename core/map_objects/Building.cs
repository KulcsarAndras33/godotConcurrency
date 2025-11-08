using core.models.descriptor;
using Godot;

public class Building : IGridObject
{
    static readonly PackedScene SPRITE_SCENE = GD.Load<PackedScene>("res://assets/BuildingScene.tscn");

    private int builtLevel = 0;
    private readonly int maxBuiltLevel = 100;
    private Node3D node; // This could later be migrated into a detailed state if needed.
    private Vector3I position;
    private readonly int descriptorId;

    private void AddBuildingToChunkSystem()
    {
        var chunkManager = ChunkManager.GetInstance();
        var chunk = chunkManager.GetChunkByPos(position);

        chunk.AddBuilding(this);
    }

    /// <summary>
    /// By default, building starts in the detailed state.
    /// </summary>
    /// <param name="position"></param>
    public Building(Vector3I position, int descriptorId)
    {
        this.position = position;
        this.descriptorId = descriptorId;
        ToDetailed();
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

    public void ToAbstract()
    {
        node.QueueFree();
    }

    public void ToDetailed()
    {
        node = SPRITE_SCENE.Instantiate<Node3D>();
        ChunkManager.GetInstance().GetTree().Root.AddChild(node);
        node.Position = position;
        node.GetNode<Sprite3D>("Sprite").Texture = GD.Load<Texture2D>(GetDescriptor().SpritePath);
    }

    public BuildingDescriptor GetDescriptor()
    {
        return Library<BuildingDescriptor>.GetInstance().GetDescriptorById(descriptorId);
    }
}