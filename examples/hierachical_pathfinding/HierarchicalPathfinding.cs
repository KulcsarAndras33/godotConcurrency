using System;
using Godot;

public partial class HierarchicalPathfinding : Node
{
    public async override void _Ready()
    {
        Vector3I chunkSize = new(40, 2, 40);
        var chunkManager = ChunkManager.CreateInstance(chunkSize);
        for (int i = 0; i < 10; i++)
        {
            chunkManager.CreateChunk(new Vector3I(i, 0, 0));
        }
        chunkManager.TransformChunks(RandomFill(0));

        GD.Print("ASD");

        await ToSignal(GetTree().CreateTimer(2), Timer.SignalName.Timeout);

        chunkManager.GenerateHierachicalPathfinding();

        await ToSignal(GetTree().CreateTimer(2), Timer.SignalName.Timeout);

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        var path = chunkManager.FindAbstractPath(new Vector3I(1, 1, 1), new Vector3I((int)9.5 * chunkSize.X, 1, 10));
        stopwatch.Stop();
        GD.Print("Pathfinding took " + stopwatch.ElapsedMilliseconds + "ms");
        GD.Print("Path length: " + path.Count);
        foreach (var step in path)
        {
            GD.Print(step);
        }

        stopwatch.Start();
        chunkManager.FindPath(new Vector3I(1, 1, 1), new Vector3I((int)9.5 * chunkSize.X, 1, 10));
        stopwatch.Stop();
        GD.Print("Detailed pathfinding took " + stopwatch.ElapsedMilliseconds + "ms");
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