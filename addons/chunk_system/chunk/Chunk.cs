
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Godot;


// TODO Nothing is setup for concurrency currently
public class Chunk
{
    private int[,,] data;

    public Vector3I dimensions;
    public Vector3I position;
    public ChunkManager chunkManager;

    private IEnumerable<Vector3I> GetEdges()
    {
        List<Vector3I> edges = new();

        for (int x = 0; x < dimensions.X; x++)
        {
            for (int y = 0; y < dimensions.Y; y++)
            {
                edges.Add(new Vector3I(x, y, 0));
                edges.Add(new Vector3I(x, y, dimensions.Z - 1));
            }
        }

        for (int x = 0; x < dimensions.X; x++)
        {
            for (int z = 0; z < dimensions.Z; z++)
            {
                edges.Add(new Vector3I(x, 0, z));
                edges.Add(new Vector3I(x, dimensions.Y - 1, z));
            }
        }

        for (int y = 0; y < dimensions.Y; y++)
        {
            for (int z = 0; z < dimensions.Z; z++)
            {
                edges.Add(new Vector3I(0, y, z));
                edges.Add(new Vector3I(dimensions.X - 1, y, z));
            }
        }

        return edges;
    }

    public List<Vector3I> GetNeighborsByEdge(Vector3I edge)
    {
        List<Vector3I> neighbors = new();
        if (edge.X == 0)
            neighbors.Add(edge + new Vector3I(-1, 0, 0));
        if (edge.X == dimensions.X - 1)
            neighbors.Add(edge + new Vector3I(1, 0, 0));
        if (edge.Y == 0)
            neighbors.Add(edge + new Vector3I(0, -1, 0));
        if (edge.Y == dimensions.Y - 1)
            neighbors.Add(edge + new Vector3I(0, 1, 0));
        if (edge.Z == 0)
            neighbors.Add(edge + new Vector3I(0, 0, -1));
        if (edge.Z == dimensions.Z - 1)
            neighbors.Add(edge + new Vector3I(0, 0, 1));

        return neighbors;
    }

    public Chunk(Vector3I dimensions, Vector3I position)
    {
        this.dimensions = dimensions;
        this.position = position;

        data = new int[dimensions.X, dimensions.Y, dimensions.Z];
    }

    public void Transform(Action<int[,,]> transformer)
    {
        transformer.Invoke(data);
    }

    public int GetData(Vector3I pos)
    {
        if (!IsInBounds(pos))
            return 0;

        return data[pos.X, pos.Y, pos.Z];
    }

    public bool IsInBounds(Vector3I pos)
    {
        return pos.X >= 0 && pos.X < dimensions.X &&
               pos.Y >= 0 && pos.Y < dimensions.Y &&
               pos.Z >= 0 && pos.Z < dimensions.Z;
    }

    public void GenerateHierarchicalPathfinding()
    {
        // Get my walkable edges
        var walkableEdges = GetEdges().Where(edge => chunkManager.IsWalkable(ToGlobal(edge))).ToList();
        // Check if neighbors have walkable edges
        foreach (var edge in walkableEdges)
        {
            var neighbors = GetNeighborsByEdge(edge);
            foreach (var neighborPos in neighbors)
            {
                if (chunkManager.IsWalkable(ToGlobal(neighborPos)))
                {
                    GD.Print("Connection from " + ToGlobal(edge) + " to " + ToGlobal(neighborPos));
                }
            }
        }

        // Create connections
    }

    public Vector3I ToGlobal(Vector3I localPos)
    {
        return localPos + position * dimensions;
    }

    public Vector3I ToLocal(Vector3I globalPos)
    {
        return globalPos - position * dimensions;
    }

}