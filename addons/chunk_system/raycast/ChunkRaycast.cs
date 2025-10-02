using System;
using Godot;

public class ChunkRaycast
{
    private Vector3 origin;
    private Vector3 direction;
    private Vector3 currentPosition;
    private Vector3 currentRatio;
    private Vector3 chunkRatio;
    private Vector3 blockRatio;
    private float maxLength;
    private ChunkManager chunkManager;

    public ChunkRaycast(Vector3 origin, Vector3 direction, float maxLength)
    {
        chunkManager = ChunkManager.GetInstance();
        this.origin = origin;
        currentPosition = origin;
        this.direction = direction;

        if (maxLength <= 0)
        {
            throw new ArgumentException("Raycast max length can't be non-positive!");
        }

        this.maxLength = maxLength;

        chunkRatio = chunkManager.GetDimensions() / direction.Abs();
        blockRatio = new Vector3(1, 1, 1) / direction.Abs();
    }

    // Returns position of collision, otherwise null
    private Vector3? BlockLevelRaycast()
    {
        var inBlockPos = currentPosition % new Vector3(1, 1, 1);
        currentRatio = inBlockPos / direction.Abs();

        // This is just an approx max length
        while (IsChunkDetailed(currentPosition) && origin.DistanceTo(currentPosition) < maxLength)
        {
            // GetDataByPos is not very efficient, as it has to always calculate which chunk the position is in
            if (chunkManager.GetDataByPos((Vector3I)currentPosition) != 0)
            {
                return currentPosition;
            }
            Jump(blockRatio);
        }

        return null;
    }

    private void ChunkLevelRaycast()
    {
        var inChunkPos = currentPosition % chunkManager.GetDimensions();
        currentRatio = inChunkPos / direction.Abs();

        // This is just an approx max length
        while (!IsChunkDetailed(currentPosition) && origin.DistanceTo(currentPosition) < maxLength)
        {
            Jump(chunkRatio);
        }
    }

    private bool IsChunkDetailed(Vector3 detailedPos) {
        var chunk = chunkManager.GetChunkByPos((Vector3I)detailedPos);
        return chunk != null && chunk.isDetailed;
    }

    private void Jump(Vector3 ratio)
    {
        var freeRatio = ratio - currentRatio;
        var minRatio = freeRatio[(int)freeRatio.MinAxisIndex()];
        currentPosition += direction * minRatio;
        currentRatio += new Vector3(minRatio, minRatio, minRatio);
        currentRatio %= ratio;
    }

    public Vector3? GetClosestHit()
    {
        while (origin.DistanceTo(currentPosition) < maxLength)
        {
            ChunkLevelRaycast();
            var possibleCollision = BlockLevelRaycast();
            if (possibleCollision != null)
            {
                return possibleCollision;
            }
        }

        return null;
    }

}