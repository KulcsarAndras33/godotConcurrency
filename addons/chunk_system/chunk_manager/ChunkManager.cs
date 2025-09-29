using System;
using System.Collections.Generic;
using System.Linq;
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

    public float GetWeightBetween(Vector3I from, Vector3I to)
    {
        if (abstractPathfinder.TryGetWeightBetween(from, to, out float weight))
        {
            return weight;
        }

        // TODO This is where some other logic can come, based on properties of the given chunk
        //      E.g.: Mountain biome -> some multiplier
        
        // Fallback to euclidean distance if chunk pathfinding was never calculated
        GD.Print("Weight NOT found");
        return from.DistanceTo(to);
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
        // TODO This might not be efficient, because we iterate through the whole abstract graph
        int startId = abstractPathfinder.GetClosestVertexId(start);
        int endId = abstractPathfinder.GetClosestVertexId(end);

        var endChunk = GetChunkByPos(end);

        var currentPath = abstractPathfinder.FindPathPositions(startId, endId);
        Vector3I? lastPathEnd = null;

        while (GetChunkByPos(currentPath.Last()) != endChunk && lastPathEnd != currentPath.Last())
        {
            lastPathEnd = currentPath.Last();
            var pathEndingChunk = GetChunkByPos(currentPath.Last());
            Chunk nextChunk = null;
            while (pathEndingChunk != endChunk)
            {
                Vector3I nextChunkPos;
                if (Math.Abs(pathEndingChunk.position.X - endChunk.position.X) > Math.Abs(pathEndingChunk.position.Z - endChunk.position.Z))
                {
                    nextChunkPos = pathEndingChunk.position + new Vector3I(1, 0, 0) * Math.Sign(endChunk.position.X - pathEndingChunk.position.X);
                }
                else
                {
                    nextChunkPos = pathEndingChunk.position + new Vector3I(0, 0, 1) * Math.Sign(endChunk.position.Z - pathEndingChunk.position.Z);
                }
                // TODO Is equals by ref suitable? (Like chunk was loaded in and out while pathfinding)
                // TODO Only working in 2 dimensions
                nextChunk = GetChunkByPos(nextChunkPos * dimensions);

                if (nextChunk != null && nextChunk.isDetailed)
                {
                    break;
                }

                currentPath.Add(nextChunkPos * dimensions);
                pathEndingChunk = nextChunk;
            }

            if (nextChunk != null && nextChunk.isDetailed)
            {
                // TODO maybe push the starting point into the direction of travel a bit?
                var partStartPos = nextChunk.position * dimensions + dimensions / 2;
                var newPathPart = abstractPathfinder.FindPathPositions(abstractPathfinder.GetClosestVertexId(partStartPos), endId);
                currentPath.AddRange(newPathPart.Skip(1));
            }
        }

        currentPath.Add(end);

        return currentPath;
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