using System.Collections.Generic;
using Godot;

public class ChunkManager
{
    static private ChunkManager instance;

    private Vector3I dimensions;
    private Dictionary<Vector3I, Chunk> chunks = new();

    public static ChunkManager GetInstance()
    {
        return instance;
    }

    public static ChunkManager CreateInstance(Vector3I dimensions)
    {
        if (instance != null)
            return instance;

        instance = new ChunkManager(dimensions);
        return instance;
    }

    private ChunkManager(Vector3I dimensions)
    {
        this.dimensions = dimensions;
    }

    public void CreateChunk(Vector3I position)
    {
        if (chunks.ContainsKey(position))
            return;

        chunks[position] = new Chunk(dimensions, position);
    }
}