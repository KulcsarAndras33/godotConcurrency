using System;
using System.Collections.Generic;
using Godot;

public class GridPathFinder : AStar<Vector3I, Vector3I>
{
    // Always one, because grid based and no diagonal movement
    protected override float Distance(Vector3I a, Vector3I b, Vector3I b_node)
    {
        return 1;
    }

    protected override Vector3I GetId(Vector3I node)
    {
        return node;
    }

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