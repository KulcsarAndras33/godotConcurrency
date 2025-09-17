using System.Collections.Generic;
using Godot;

public class GridPathFinder : AStar<Vector3I>
{
    // TODO No up and down
    override protected List<Vector3I> GetNeighbors(Vector3I node)
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

    override protected float Heuristic(Vector3I a, Vector3I b)
    {
        return a.DistanceTo(b);
    }
}