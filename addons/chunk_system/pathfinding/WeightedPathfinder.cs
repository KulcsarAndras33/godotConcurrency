using System.Collections.Generic;
using System.Numerics;
using Godot;

public class WeightedPathfinder : StopableAStar<int, Edge>
{
    // This is needed, otherwise the heuristic is too optimistic and A* behaves like Dijkstra
    const float HEURISTIC_FACTOR = 1.7f;
    const float STOP_THRESHOLD_PERCENT = 1.2f;

    private readonly Dictionary<int, List<Edge>> edges = new();
    private readonly Dictionary<int, Vector3I> vertices = new();

    public WeightedPathfinder()
    {
        isWalkable = _ => true;
    }

    protected override float Distance(int a, int b, Edge b_node)
    {
        return b_node.weight;
    }

    protected override int GetId(Edge node)
    {
        return node.to;
    }

    override protected List<Edge> GetNeighbors(int node)
    {
        return edges.GetValueOrDefault(node, new List<Edge>());
    }

    protected override float Heuristic(int a, int b)
    {
        return vertices[a].DistanceTo(vertices[b]) * HEURISTIC_FACTOR;
    }

    public int GetVertexId(Vector3I vertex)
    {
        return vertex.GetHashCode();
    }

    public int GetClosestVertexId(Vector3I vertex)
    {
        int closestId = -1;
        float closestDistance = float.MaxValue;

        foreach (var kvp in vertices)
        {
            float distance = kvp.Value.DistanceTo(vertex);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestId = kvp.Key;
            }
        }

        return closestId;
    }

    public void AddEdge(Vector3I from, Vector3I to, float weight, bool bidirectional = true)
    {
        AddDirectedEdge(from, to, weight);
        if (bidirectional)
            AddDirectedEdge(to, from, weight);
    }

    public void AddDirectedEdge(Vector3I from, Vector3I to, float weight)
    {
        if (from.Equals(to))
        {
            GD.PrintErr("Trying to add self-loop edge, ignoring");
            return;
        }

        int fromId = GetVertexId(from);
        int toId = GetVertexId(to);

        if (!vertices.ContainsKey(fromId))
            vertices[fromId] = from;

        if (!vertices.ContainsKey(toId))
            vertices[toId] = to;

        if (!edges.ContainsKey(fromId))
            edges[fromId] = new List<Edge>();

        edges[fromId].Add(new Edge(toId, weight));
    }

    public List<Vector3I> FindPathPositions(int start, int end)
    {
        var pathIds = FindPath(start, end);
        var path = new List<Vector3I>();
        foreach (var id in pathIds)
        {
            path.Add(vertices[id]);
        }
        return path;
    }

    public bool TryGetWeightBetween(Vector3I from, Vector3I to, out float weight)
    {
        int fromId = GetVertexId(from);
        int toId = GetVertexId(to);

        if (edges.TryGetValue(fromId, out var edgeList))
        {
            foreach (var edge in edgeList)
            {
                if (edge.to == toId)
                {
                    weight = edge.weight;
                    return true;
                }
            }
        }

        weight = 0;
        return false;
    }

    public Vector3I GetPositionById(int id)
    {
        return vertices[id];
    }

    protected override bool StopCondition(int current, float currentFScore, int best, float bestFScore)
    {
        return currentFScore >= bestFScore * STOP_THRESHOLD_PERCENT;
    }

}