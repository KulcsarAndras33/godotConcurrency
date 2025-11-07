using System;
using System.Threading.Tasks;
using Godot;
namespace Example
{

    public partial class NeverloadedPathfinding : Node
    {
        public async override void _Ready()
        {
            Vector3I chunkSize = new(20, 2, 20);
            var chunkManager = ChunkManager.CreateInstance(chunkSize);
            AddChild(chunkManager);

            var map = @"
11110011
10000010
10000010
11111110
        ";

            await CreateMapByString(chunkManager, map);

            var visualiser = GetNode<NaiveChunkVisualiser>("NaiveChunkVisualiser");
            visualiser.Create(chunkManager);

            var pathDrawer = GetNode<PathVisualiser>("PathVisualiser");

            var communityManager = new CommunityManager();
            AddChild(communityManager);
            const int AGENT_COUNT = 1;
            for (int i = 0; i < AGENT_COUNT; i++)
            {
                var agent = new MovingAgent();

                var action = new MoveAction();
                action.abstractPath = chunkManager.FindAbstractPath(new Vector3I(0, 1, 5), new Vector3I((int)(7.5 * chunkSize.X), 1, 5));
                action.agent = agent;

                agent.SetActions([action]);
                pathDrawer.SetAgent(agent);
                communityManager.AddAgent(agent);
            }
        }

        public async Task<object> CreateMapByString(ChunkManager manager, string map)
        {
            var lines = map.Split('\n');
            var x = 0;
            var z = -1;

            foreach (var line in lines)
            {
                x = 0;
                foreach (char c in line)
                {
                    if (c == '1')
                    {
                        manager.CreateChunk(new Vector3I(x, 0, z));
                    }
                    x++;
                }
                z++;
            }

            manager.TransformChunks(RandomFill(0));

            await ToSignal(GetTree().CreateTimer(1), Timer.SignalName.Timeout);

            manager.GenerateHierachicalPathfinding();

            await ToSignal(GetTree().CreateTimer(1), Timer.SignalName.Timeout);

            x = 0;
            z = -1;

            foreach (var line in lines)
            {
                x = 0;
                foreach (char c in line)
                {
                    if (c == '0')
                    {
                        manager.CreateChunk(new Vector3I(x, 0, z));
                    }
                    x++;
                }
                z++;
            }

            return () => { };
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
}