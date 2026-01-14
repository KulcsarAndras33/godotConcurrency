namespace ChunkSystem.Observation
{
    public interface IObservation
    {
        public ObservationResult Observe(ChunkManager chunkManager);
    }
}