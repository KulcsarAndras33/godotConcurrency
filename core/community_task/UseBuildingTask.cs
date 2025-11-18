using System;
using System.Collections.Generic;
using Godot;

public class UseBuildingTask : BaseCommunityTask
{
    private readonly float MAX_BUILD_DISTANCE = 1.5f;
    // TODO Temporary
    private readonly Vector3I BUILDING_MOVE_OFFSET = new Vector3I(0, 0, 1);

    public Building building;

    public UseBuildingTask(Building building)
    {
        this.building = building;
    }

    private MoveAction CreateMoveAction(MovingAgent agent, Vector3I buildingPos)
    {
        return new MoveAction
        {
            abstractPath = ChunkManager.GetInstance().FindAbstractPath((Vector3I)agent.GetPosition(), buildingPos + BUILDING_MOVE_OFFSET),
            agent = agent
        };
    }

    public override List<AgentAction> GetActions(IAgent agent)
    {
        if (agent is not MovingAgent)
        {
            throw new ArgumentException("Agent for BuildTask shall be MovingAgent.");
        }

        List<AgentAction> actions = [];
        var movingAgent = agent as MovingAgent;

        if (movingAgent.GetPosition().DistanceTo(building.GetPosition()) > MAX_BUILD_DISTANCE)
        {
            actions.Add(CreateMoveAction(movingAgent, building.GetPosition()));
        }

        actions.Add(new UseBuildingAction() { building = building });

        return actions;
    }

    public override bool IsApplicable(IAgent agent)
    {
        // TODO Evaluate
        return true;
    }

    public override bool IsCompleted()
    {
        // TODO Think about this
        return false;
    }
}