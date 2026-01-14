using Godot;

namespace ChunkSystem.Observation
{
    public class SphereObservation(Vector3 origin, float radius) : ObservationBase
    {
        private Vector3 origin = origin;
        private readonly float radius = radius;

        protected override bool IsChunkIn(Vector3 chunkCenter, float diagonal)
        {
            return origin.DistanceTo(chunkCenter) - diagonal <= radius;
        }

        protected override Chunk GetStartingChunk(ChunkManager chunkManager)
        {
            return chunkManager.GetChunkByPos(new Vector3I((int)origin.X, (int)origin.Y, (int)origin.Z));
        }

        protected override bool IsInRange(IAgent agent)
        {
            return origin.DistanceTo(agent.GetPosition()) <= radius;
        }

        protected override bool IsInRange(Building building)
        {
            return origin.DistanceTo(building.GetPosition()) <= radius;
        }
    }
}