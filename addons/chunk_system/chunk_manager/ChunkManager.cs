using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public class ChunkManager
{
    static private ChunkManager instance;

    private Vector3I dimensions;
    private Dictionary<Vector3I, Chunk> chunks = new();
    private PriorityThreadPool threadPool = new(4);

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

    public void TransformChunks(Action<int[,,]> transformer)
    {
        foreach (var chunk in chunks.Values)
        {
            threadPool.Enqueue(() => 
            {
                chunk.Transform(transformer);
            });
        }
    }

    public Chunk GetChunkByPos(Vector3I pos)
    {
        Vector3I chunkPos = pos / dimensions;
        return chunks.GetValueOrDefault(chunkPos, null);
    }

    public int GetDataByPos(Vector3I pos)
    {
        Chunk chunk = GetChunkByPos(pos);
        if (chunk == null)
        {
            return 0;
        }

        return chunk.GetData(pos % dimensions);
    }

    public bool IsWalkable(Vector3I pos)
    {
        int data = GetDataByPos(pos + new Vector3I(0, -1, 0));
        return data != 0;
    }

    public List<Vector3I> FindPath(Vector3I start, Vector3I end)
    {
        return GridPathFinder.FindPath(start, end, IsWalkable);
    }
}