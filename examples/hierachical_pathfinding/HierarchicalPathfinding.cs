using System;
using Godot;

public partial class HierarchicalPathfinding : Node
{
    public async override void _Ready()
    {
        Vector3I chunkSize = new(20, 2, 20);
        var chunkManager = ChunkManager.CreateInstance(chunkSize);
        chunkManager.CreateChunk(new Vector3I(0, 0, 0));
        chunkManager.CreateChunk(new Vector3I(1, 0, 0));
        chunkManager.TransformChunks(RandomFill(0));

        GD.Print("ASD");

        await ToSignal(GetTree().CreateTimer(1), Timer.SignalName.Timeout);
        
        chunkManager.GenerateHierachicalPathfinding();

    }

    public Action<int[,,]> RandomFill(int level)
    {
        return data =>
        {
            var rand = new Random();
            for (int x = 0; x < data.GetLength(0); x++)
            {
                for (int z = 0; z < data.GetLength(2); z++)
                {
                    if (rand.NextDouble() < 0.9)
                    {
                        data[x, level, z] = 1;
                    }
                }
            }
        };
    }
}