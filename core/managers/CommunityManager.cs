using System;
using System.Collections.Generic;
using Godot;

public partial class CommunityManager : Node
{
    private Random random = new();
    private List<IAgent> agents = new();

    public Vector3I GetRandomGoal()
    {
        GD.Print("Getting random goal");
        return new Vector3I(random.Next(100), 1, random.Next(100));
    }

    public void AddAgent(IAgent agent)
    {
        agent.communityManager = this;
        agents.Add(agent);
    }

    public override void _Process(double delta)
    {
        foreach (var agent in agents)
        {
            agent.Tick();
        }
    }
}