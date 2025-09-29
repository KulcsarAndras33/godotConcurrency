using System;
using System.Collections.Generic;

public abstract class AStar<T, R> where T : IEquatable<T>
{
    public Func<T, bool> isWalkable { get; set; }

    protected List<T> ReconstructPath(Dictionary<T, T> cameFrom, T current)
    {
        var totalPath = new List<T> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }

    abstract protected List<R> GetNeighbors(T node);

    abstract protected T GetId(R node);

    abstract protected float Heuristic(T a, T b);

    abstract protected float Distance(T a, T b, R b_node);

    public List<T> FindPath(T start, T end)
    {
        if (isWalkable == null)
            throw new InvalidOperationException("isWalkable function must be set before calling FindPath.");

        var openSet = new PriorityQueue<T, float>();
        var cameFrom = new Dictionary<T, T>();
        var gScore = new Dictionary<T, float>();
        var fScore = new Dictionary<T, float>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, end);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current.Equals(end))
                return ReconstructPath(cameFrom, current);
            
            foreach (var neighbor in GetNeighbors(current))
            {
                var neighborId = GetId(neighbor);
                if (!isWalkable(neighborId))
                    continue;

                float tentativeGScore = gScore[current] + Distance(current, neighborId, neighbor);

                if (!gScore.ContainsKey(neighborId) || tentativeGScore < gScore[neighborId])
                {
                    cameFrom[neighborId] = current;
                    gScore[neighborId] = tentativeGScore;
                    fScore[neighborId] = tentativeGScore + Heuristic(neighborId, end);
                    openSet.Enqueue(neighborId, fScore[neighborId]);
                }
            }
        }

        return new List<T>(); // No path found
    }


}