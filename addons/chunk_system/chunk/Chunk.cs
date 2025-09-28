
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

// TODO Nothing is setup for concurrency currently
public class Chunk
{
    private int[,,] data;
    private List<IAgent> agents = new();

    public Vector3I dimensions;
    public Vector3I position;
    public ChunkManager chunkManager;
    public bool isDetailed { get; private set; } = false;
    public bool isPathFindingCalculated { get; private set; } = false;

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

    private Dictionary<Vector3I, int> InChunkFloodFill(Vector3I start)
    {
        return FloodFillUtil.FloodFill(start, pos =>
        {
            var neighbors = new List<Vector3I>
            {
                // TODO No up and down
                pos + new Vector3I(1, 0, 0),
                pos + new Vector3I(-1, 0, 0),
                pos + new Vector3I(0, 0, 1),
                pos + new Vector3I(0, 0, -1)
            };

            return neighbors.Where(n => IsInBounds(n) && chunkManager.IsWalkable(ToGlobal(n)));
        });
    }

    private List<(Vector3I, Vector3I)> FilterConnections(List<(Vector3I, Vector3I)> connections)
    {
        List<HashSet<(Vector3I, Vector3I)>> groups = new();
        foreach (var connection in connections)
        {
            bool foundGroup = false;
            foreach (var group in groups)
            {
                if (ChunkUtil.IsConnectionInGroup(connection, group))
                {
                    group.Add(connection);
                    foundGroup = true;
                    break;
                }
            }

            if (!foundGroup)
            {
                var newGroup = new HashSet<(Vector3I, Vector3I)>
                {
                    connection
                };
                groups.Add(newGroup);
            }
        }

        List<(Vector3I, Vector3I)> filtered = new();
        foreach (var group in groups)
        {
            var sorted = group.ToList();
            sorted.Sort((a, b) => ChunkUtil.VectorCompare(a.Item1, b.Item1));
            int[] indexes = [0, sorted.Count / 2, sorted.Count - 1];
            for (int i = 0; i < Math.Min(3, sorted.Count); i++)
            {
                filtered.Add(sorted[indexes[i]]);
            }
        }

        return filtered;
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
    }

    public void Transform(Action<int[,,]> transformer)
    {
        isPathFindingCalculated = false;

        // TODO Maybe move this to a more explicit method
        //      to show that this is part of becoming detailed.
        data = new int[dimensions.X, dimensions.Y, dimensions.Z];

        transformer.Invoke(data);

        isDetailed = true;
    }

    public int GetData(Vector3I pos)
    {
        if (!IsInBounds(pos) || !isDetailed)
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
        var connections = new List<(Vector3I from, Vector3I to)>();
        foreach (var edge in walkableEdges)
        {
            var neighbors = GetNeighborsByEdge(edge);
            foreach (var neighborPos in neighbors)
            {
                if (chunkManager.IsWalkable(ToGlobal(neighborPos)))
                {
                    connections.Add((edge, neighborPos));
                }
            }
        }

        // Create connections
        connections = FilterConnections(connections);
        foreach (var connection in connections)
        {
            chunkManager.AddAbstractEdge(ToGlobal(connection.from), ToGlobal(connection.to), 1, false);

            var distances = InChunkFloodFill(connection.from);
            foreach (var other in connections)
            {
                if (other.from.Equals(connection.from))
                    continue;

                if (distances.TryGetValue(other.from, out int distance))
                {
                    chunkManager.AddAbstractEdge(ToGlobal(connection.from), ToGlobal(other.from), distance, false);
                }
            }
        }

        isPathFindingCalculated = true;
    }

    public Vector3I ToGlobal(Vector3I localPos)
    {
        return localPos + position * dimensions;
    }

    public Vector3I ToLocal(Vector3I globalPos)
    {
        return globalPos - position * dimensions;
    }

    public void AddAgent(IAgent agent)
    {
        agents.Add(agent);
    }

    public void RemoveAgent(IAgent agent)
    {
        agents.Remove(agent);
    }

    public void ToAbstract()
    {
        isDetailed = false;
        data = null;
    }
}