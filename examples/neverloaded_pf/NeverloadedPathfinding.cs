using System;
using Godot;

public partial class NeverloadedPathfinding : Node
{
    public async override void _Ready()
    {
        Vector3I chunkSize = new(20, 2, 20);
        var chunkManager = ChunkManager.CreateInstance(chunkSize);
        for (int i = 0; i < 5; i++)
        {
            chunkManager.CreateChunk(new Vector3I(i, 0, 0));
        }
        chunkManager.TransformChunks(RandomFill(0));

        GD.Print("ASD");

        await ToSignal(GetTree().CreateTimer(1), Timer.SignalName.Timeout);

        GD.Print("Generating PF");
        chunkManager.GenerateHierachicalPathfinding();
        GD.Print("Generated PF");

        await ToSignal(GetTree().CreateTimer(1), Timer.SignalName.Timeout);

        // Adding never-loaded chunk
        GD.Print("Adding never loaded chunks");
        chunkManager.CreateChunk(new Vector3I(6, 0, 0));
        chunkManager.CreateChunk(new Vector3I(7, 0, 0));
        GD.Print("Added never loaded chunks");

        await ToSignal(GetTree().CreateTimer(1), Timer.SignalName.Timeout);

        var path = chunkManager.FindAbstractPath(new Vector3I(1, 1, 1), new Vector3I((int)4 * chunkSize.X, 1, 10));
        GD.Print("Path length: " + path.Count);
        foreach (var step in path)
        {
            GD.Print(step);
        }
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
                    if (rand.NextDouble() < 1.5)
                    {
                        data[x, level, z] = 1;
                    }
                }
            }
        };
    }
}