using System;
using Godot;

public partial class NeverloadedPathfinding : Node
{
    public async override void _Ready()
    {
        Vector3I chunkSize = new(20, 2, 20);
        var chunkManager = ChunkManager.CreateInstance(chunkSize);
        AddChild(chunkManager);

        for (int i = 0; i < 5; i++)
        {
            chunkManager.CreateChunk(new Vector3I(i, 0, 0));
        }

        chunkManager.CreateChunk(new Vector3I(7, 0, 0));
        chunkManager.CreateChunk(new Vector3I(8, 0, 0));

        chunkManager.TransformChunks(RandomFill(0));

        GD.Print("ASD");

        await ToSignal(GetTree().CreateTimer(1), Timer.SignalName.Timeout);

        GD.Print("Generating PF");
        chunkManager.GenerateHierachicalPathfinding();
        GD.Print("Generated PF");

        await ToSignal(GetTree().CreateTimer(1), Timer.SignalName.Timeout);

        GD.Print("Adding never loaded chunks");
        chunkManager.CreateChunk(new Vector3I(5, 0, 0));
        chunkManager.CreateChunk(new Vector3I(6, 0, 0));
        GD.Print("Added never loaded chunks");

        await ToSignal(GetTree().CreateTimer(1), Timer.SignalName.Timeout);

        var visualiser = GetNode<NaiveChunkVisualiser>("NaiveChunkVisualiser");
        visualiser.Create(chunkManager);

        var pathDrawer = GetNode<PathVisualiser>("PathVisualiser");

        var communityManager = new CommunityManager();
        AddChild(communityManager);
        const int AGENT_COUNT = 1;
        for (int i = 0; i < AGENT_COUNT; i++) {
            var agent = new MovingAgent();

            var action = new MoveAction();
            action.abstractPath = chunkManager.FindAbstractPath(new Vector3I(0, 1, 5), new Vector3I((int)(7.5 * chunkSize.X), 1, 5));
            action.agent = agent;

            agent.SetAction(action);
            pathDrawer.SetAgent(agent);
            communityManager.AddAgent(agent);
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