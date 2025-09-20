using Godot;

public partial class MovingAgent : IAgent
{
    private IMovingState currentState;
    private AgentAction currentAction;
    public CommunityManager communityManager { get; set; }

    public MovingAgent()
    {
        currentState = new MovingAgentDetailedState(this, new Vector3(0, 1, 0));
        currentState.Load();
        currentState.Enter();
    }

    public void Tick()
    {
        if (currentAction == null || currentAction.IsComplete())
        {
            currentState.GetDefaultAction(action =>
            {
                currentAction = action;
            });
        }
        else
        {
            if (!currentAction.IsOnCoolDown())
            {
                currentAction.DoDetailedStep();
            }
        }
    }

    public void SetPosition(Vector3 position)
    {
        currentState.SetPostion(position);
    }

    public Vector3 GetPosition()
    {
        return currentState.GetPostion();
    }
}