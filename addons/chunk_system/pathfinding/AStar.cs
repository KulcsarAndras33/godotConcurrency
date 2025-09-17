using System;
using System.Collections.Generic;
using System.Linq;

public abstract class AStar<T> where T : IEquatable<T>
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

    abstract protected List<T> GetNeighbors(T node);

    abstract protected float Heuristic(T a, T b);

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
                if (!isWalkable(neighbor))
                    continue;

                float tentativeGScore = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor, end);

                    if (!openSet.UnorderedItems.Any(item => item.Element.Equals(neighbor)))
                    {
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }
        }

        return new List<T>(); // No path found
    }


}