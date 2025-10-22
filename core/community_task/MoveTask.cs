using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MoveTask : BaseCommunityTask
{
    private Vector3I goalPos;

    public MoveTask(Vector3I goalPos)
    {
        this.goalPos = goalPos;
    }

    public override bool IsApplicable(IAgent agent)
    {
        return agent is MovingAgent;
    }

    public override bool IsCompleted()
    {
        return currentAgents.Select(agent => agent as MovingAgent).All(agent => agent.GetPosition().DistanceTo(goalPos) < 0.1);
    }

    public override List<AgentAction> GetActions(IAgent agent)
    {
        if (agent is not MovingAgent)
        {
            throw new ArgumentException("Agent for MoveTask shall be MovingAgent.");
        }
        var movingAgent = agent as MovingAgent;

        var action = new MoveAction
        {
            abstractPath = ChunkManager.GetInstance().FindAbstractPath((Vector3I)movingAgent.GetPosition(), goalPos),
            agent = movingAgent
        };
        return [action];
    }
    
    public override float GetPriority()
    {
        return 10;
    }
}