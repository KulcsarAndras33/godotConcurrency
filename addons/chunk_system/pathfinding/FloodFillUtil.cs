using System;
using System.Collections.Generic;
using Godot;

public class FloodFillUtil
{
    public static Dictionary<Vector3I, int> FloodFill(Vector3I start, Func<Vector3I, IEnumerable<Vector3I>> getNeighbors)
    {
        var visited = new HashSet<Vector3I>();
        var toVisit = new Queue<Vector3I>();
        var distances = new Dictionary<Vector3I, int>();

        toVisit.Enqueue(start);
        visited.Add(start);
        distances[start] = 0;

        while (toVisit.Count > 0)
        {
            var current = toVisit.Dequeue();

            foreach (var neighbor in getNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    // Assuming uniform cost for simplicity; modify as needed
                    distances[neighbor] = distances[current] + 1;
                    visited.Add(neighbor);
                    toVisit.Enqueue(neighbor);
                }
            }
        }

        return distances;
    }
}