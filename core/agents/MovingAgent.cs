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
        if (!currentState.IsValid())
        {
            currentState = currentState.GetNextState() as IMovingState;
            GD.Print("State changed to: " + currentState.GetType().Name);
        }

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
                if (currentState.IsDetailed())
                {
                    currentAction.DoDetailedStep();
                }
                else
                {
                    currentAction.DoAbstractStep();
                }
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