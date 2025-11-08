using System;
using Godot;
namespace Example
{

    public partial class RaycastEg : Node
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
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton eventMouseButton)
            {
                if (eventMouseButton.Pressed)
                    return;
                var camera = GetNode<Camera3D>("Camera3D");
                var point = camera.ProjectPosition(eventMouseButton.Position, 1);
                var dir = point - camera.GlobalPosition;
                var rayCast = new ChunkRaycast(camera.GlobalPosition, dir.Normalized(), 150);
                var collision = rayCast.GetClosestHit();
                if (collision != null)
                {
                    GD.Print((Vector3I)collision);
                }
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
}