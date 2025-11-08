using System.Collections.Generic;
using System.Linq;
using core.models.descriptor;
using Godot;

public partial class MovingAgent : IAgent
{
    private readonly int descriptorId;
    private IMovingState currentState;
    private List<AgentAction> currentActions = [];

    public CommunityManager communityManager { get; set; }
    public Chunk CurrentChunk { get; set; }

    public MovingAgent(int descriptorId)
    {
        this.descriptorId = descriptorId;

        currentState = new MovingAgentDetailedState(this, new Vector3(0, 1, 0));
        currentState.Load();
        currentState.Enter();
    }

    public void Tick()
    {
        if (!currentState.IsValid())
        {
            GD.Print("State change");
            currentState = currentState.GetNextState() as IMovingState;
        }

        if (currentActions.Count == 0)
        {
            currentState.NoActionLeft();
            currentActions.Add(new IdleAction());
        }

        var currentAction = currentActions.First();

        if (currentAction == null || currentAction.IsComplete())
        {
            currentActions.RemoveAt(0);
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

    public void SetActions(List<AgentAction> actions)
    {
        currentActions = actions;
    }

    public Vector3 GetPosition()
    {
        return currentState.GetPostion();
    }

    public MoveAction GetMoveAction()
    {
        if (currentActions.Count == 0 || currentActions.First() is not MoveAction)
        {
            return null;
        }

        return currentActions.First() as MoveAction;
    }

    public void ToAbstract()
    {
        // This works based on the assumption that the MovingAgent only has two states,
        //      one detailed, and one abstract.
        if (currentState.IsDetailed())
        {
            currentState = currentState.GetNextState() as IMovingState;
        }
        currentActions.First().HandleStateChange();
    }

    public void ToDetailed()
    {
        // This works based on the assumption that the MovingAgent only has two states,
        //      one detailed, and one abstract.
        if (!currentState.IsDetailed())
        {
            currentState = currentState.GetNextState() as IMovingState;
        }
        currentActions.First().HandleStateChange();
    }

    public AgentDescriptor GetDescriptor()
    {
        return Library<AgentDescriptor>.GetInstance().GetDescriptorById(descriptorId);
    }
}