using System.Collections.Generic;
using Godot;

public class MoveAction : AgentAction
{
    private bool isComplete = false;
    private SingleTaskExecutor executor = new(ChunkManager.GetInstance().threadPool);

    public List<Vector3I> abstractPath { get; set; }
    public List<Vector3I> detailedPath { get; set; }
    public MovingAgent agent { get; set; }

    private void TakeStep()
    {
        agent.SetPosition(detailedPath[0]);
        detailedPath.RemoveAt(0);
    }

    protected override void DetailedNextStep()
    {
        if (detailedPath != null && detailedPath.Count > 0 && !executor.IsExecuting())
        {
            TakeStep();
            return;
        }

        if (abstractPath == null || abstractPath.Count == 0)
        {
            // No more steps to take
            isComplete = true;
            return;
        }

        // If the next abstract node is just a step away, then take that step
        // This helps with proper state changing
        if (((Vector3)abstractPath[0]).DistanceTo(agent.GetPosition()) < 1.1)
        {
            agent.SetPosition(abstractPath[0]);
            abstractPath.RemoveAt(0);
            return;
        }

        executor.TryExecute(() =>
        {
            detailedPath = ChunkManager.GetInstance().FindPath((Vector3I)agent.GetPosition(), abstractPath[0]);
            detailedPath.RemoveAt(0); // Remove first pos from path, since path contains start pos
            abstractPath.RemoveAt(0);
        });
    }

    protected override void AbstractNextStep()
    {
        if (abstractPath == null || abstractPath.Count == 0)
        {
            // No more steps to take
            isComplete = true;
            return;
        }

        agent.SetPosition(abstractPath[0]);
        abstractPath.RemoveAt(0);
    }

    public override bool IsComplete()
    {
        return isComplete;
    }

    protected override ulong GetDetailedTimeout()
    {
        return (ulong)(executor.IsExecuting() ? 0 : 300);
    }

    protected override ulong GetAbstractTimeout()
    {
        if (abstractPath == null || abstractPath.Count == 0)
            return 0;
        
        return (ulong) ChunkManager.GetInstance().GetWeightBetween((Vector3I)agent.GetPosition(), abstractPath[0]) * 300;
    }
}