using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChunkUtil
{
    public static bool IsConnectionInGroup((Vector3I from, Vector3I to) connection, HashSet<(Vector3I, Vector3I)> group)
    {
        var first = group.First();
        if (first.Item2 - first.Item1 != connection.to - connection.from)
            return false;

        foreach (var conn in group)
        {
            if (conn.Item1.DistanceTo(connection.from) < 2)
            {
                return true;
            }
        }
        return false;
    }
    
    public static int VectorCompare(Vector3I a, Vector3I b)
    {
        if (a.X != b.X)
            return a.X.CompareTo(b.X);
        if (a.Y != b.Y)
            return a.Y.CompareTo(b.Y);
        return a.Z.CompareTo(b.Z);
    }
}