
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

}