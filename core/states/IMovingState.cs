using Godot;

public interface IMovingState : IState
{
    void SetPostion(Vector3 position);
    Vector3 GetPostion();
}