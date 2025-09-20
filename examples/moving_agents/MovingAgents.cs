using System;
using Godot;

public partial class MovingAgents : Node
{
    public async override void _Ready()
    {
        Vector3I chunkSize = new(20, 2, 20);
        var chunkManager = ChunkManager.CreateInstance(chunkSize);
        AddChild(chunkManager);
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                chunkManager.CreateChunk(new Vector3I(i, 0, j));
            }
        }
        chunkManager.TransformChunks(RandomFill(0));

        GD.Print("ASD");

        await ToSignal(GetTree().CreateTimer(1), Timer.SignalName.Timeout);

        chunkManager.GenerateHierachicalPathfinding();

        await ToSignal(GetTree().CreateTimer(1), Timer.SignalName.Timeout);

        for (int x = 0; x < 5; x++)
        {
            for (int z = 0; z < 5; z++)
            {
                if ((x + z) % 2 == 1)
                {
                    chunkManager.SetChunkToAbstract(new Vector3I(x, 0, z) * chunkSize);
                }
            }
        }

        var visualiser = GetNode<NaiveChunkVisualiser>("NaiveChunkVisualiser");
        visualiser.Create(chunkManager);

        var communityManager = new CommunityManager();
        AddChild(communityManager);
        const int AGENT_COUNT = 1;
        for (int i = 0; i < AGENT_COUNT; i++) {
            communityManager.AddAgent(new MovingAgent());
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
                    if (rand.NextDouble() < 0.9)
                    {
                        data[x, level, z] = 1;
                    }
                }
            }
        };
    }
}