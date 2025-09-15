using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Godot;

public class GridPathFinder
{
    static private List<Vector3I> ReconstructPath(Dictionary<Vector3I, Vector3I> cameFrom, Vector3I current)
    {
        var totalPath = new List<Vector3I> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }

    static private List<Vector3I> GetNeighbors(Vector3I node)
    {
        var neighbors = new List<Vector3I>
        {
            node + new Vector3I(1, 0, 0),
            node + new Vector3I(-1, 0, 0),
            node + new Vector3I(0, 0, 1),
            node + new Vector3I(0, 0, -1)
        };
        return neighbors;
    }

    static private float Heuristic(Vector3I a, Vector3I b)
    {
        return a.DistanceTo(b);
    }   

    static public List<Vector3I> FindPath(Vector3I start, Vector3I end, System.Func<Vector3I, bool> isWalkable)
    {
        var openSet = new PriorityQueue<Vector3I, float>();
        var cameFrom = new Dictionary<Vector3I, Vector3I>();
        var gScore = new Dictionary<Vector3I, float>();
        var fScore = new Dictionary<Vector3I, float>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, end);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current == end)
                return ReconstructPath(cameFrom, current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!isWalkable(neighbor))
                    continue;

                float tentativeGScore = gScore[current] + 1; // Assuming uniform cost

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor, end);

                    if (!openSet.UnorderedItems.Any(item => item.Element == neighbor))
                    {
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }
        }

        return new List<Vector3I>(); // No path found
    }


}