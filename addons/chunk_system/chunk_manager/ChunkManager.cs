using System;
using System.Collections.Generic;
using System.Threading;
using Godot;

public partial class ChunkManager : Node
{
    static private ChunkManager instance;

    private Vector3I dimensions;
    private Dictionary<Vector3I, Chunk> chunks = new();
    private GridPathFinder gridPathFinder = new();
    private WeightedPathfinder abstractPathfinder = new();
    private ReaderWriterLock abstractPathfinderLock = new();

    public PriorityThreadPool threadPool = new(10);

    public static ChunkManager GetInstance()
    {
        return instance;
    }

    public static ChunkManager CreateInstance(Vector3I dimensions)
    {
        if (instance != null)
            return instance;

        instance = new ChunkManager(dimensions);
        instance.gridPathFinder.isWalkable = instance.IsWalkable;
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
        chunks[position].chunkManager = this;
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

    public void GenerateHierachicalPathfinding()
    {
        foreach (var chunk in chunks.Values)
        {
            threadPool.Enqueue(() =>
            {
                chunk.GenerateHierarchicalPathfinding();
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

    public IEnumerable<Chunk> GetChunks()
    {
        return chunks.Values;
    }

    public bool IsWalkable(Vector3I pos)
    {
        int data = GetDataByPos(pos + new Vector3I(0, -1, 0));
        return data != 0;
    }

    public List<Vector3I> FindPath(Vector3I start, Vector3I end)
    {
        var path = gridPathFinder.FindPath(start, end);
        return path;
    }

    public List<Vector3I> FindAbstractPath(Vector3I start, Vector3I end)
    {
        int startId = abstractPathfinder.GetClosestVertexId(start);
        int endId = abstractPathfinder.GetClosestVertexId(end);

        return abstractPathfinder.FindPathPositions(startId, endId);
    }

    public void AddAbstractEdge(Vector3I from, Vector3I to, float weight, bool bidirectional = true)
    {
        abstractPathfinderLock.AcquireWriterLock(int.MaxValue);
        abstractPathfinder.AddEdge(from, to, weight, bidirectional);
        abstractPathfinderLock.ReleaseWriterLock();
    }

    public void SetChunkToAbstract(Vector3I globalPos)
    {
        var chunk = GetChunkByPos(globalPos);
        if (chunk != null && chunk.isDetailed)
        {
            chunk.ToAbstract();
            GD.Print($"Set chunk at {chunk.position} to abstract");
        }
    }
}