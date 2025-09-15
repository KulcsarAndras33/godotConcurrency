
using System;
using Godot;

public class Chunk
{
    private int[,,] data;

    public Vector3I dimensions;
    public Vector3I position;

    public Chunk(Vector3I dimensions, Vector3I position)
    {
        this.dimensions = dimensions;
        this.position = position;

        data = new int[dimensions.X, dimensions.Y, dimensions.Z];
    }

    public void Transform(Action<int[,,]> transformer)
    {
        transformer.Invoke(data);

        // TODO Try get entrances
    }

    public int GetData(Vector3I pos)
    {
        if (!IsInBounds(pos))
            return 0;
        
        return data[pos.X, pos.Y, pos.Z];
    }

    public bool IsInBounds(Vector3I pos) {
        return pos.X >= 0 && pos.X < dimensions.X &&
               pos.Y >= 0 && pos.Y < dimensions.Y &&
               pos.Z >= 0 && pos.Z < dimensions.Z;
    }

}