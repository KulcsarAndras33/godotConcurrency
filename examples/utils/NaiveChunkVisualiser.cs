using Godot;

public partial class NaiveChunkVisualiser : Node3D
{
    static PackedScene nodeScene = GD.Load<PackedScene>("res://examples/utils/Node.tscn");

    public void Create(ChunkManager chunkManager)
    {
        foreach (Chunk chunk in chunkManager.GetChunks())
        {
            if (!chunk.isDetailed)
                continue;
            for (int x = 0; x < chunk.dimensions.X; x++)
            {
                for (int y = 0; y < chunk.dimensions.Y; y++)
                {
                    for (int z = 0; z < chunk.dimensions.Z; z++)
                    {
                        var currPos = new Vector3I(x, y, z);
                        if (chunk.GetData(currPos) != 0)
                        {
                            var instance = nodeScene.Instantiate<Node3D>();
                            AddChild(instance);
                            instance.GlobalPosition = chunk.dimensions * chunk.position + currPos;
                        }
                    }
                }
            }
        }
    }
}