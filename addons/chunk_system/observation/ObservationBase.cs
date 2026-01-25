using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ChunkSystem.Observation
{
    public abstract class ObservationBase : IObservation
    {
        protected bool onlyAgent = false;
        protected Func<IAgent, bool> agentFilter;

        protected abstract bool IsChunkIn(Vector3 chunkCenter, float diagonal);
        protected abstract Chunk GetStartingChunk(ChunkManager chunkManager);
        protected abstract bool IsInRange(IAgent agent);
        protected abstract bool IsInRange(Building building);

        public ObservationResult Observe(ChunkManager chunkManager)
        {
            ObservationResult result = new();
            HashSet<Chunk> checkedChunks = [];

            Queue<Chunk> toBeChecked = new();
            toBeChecked.Enqueue(GetStartingChunk(chunkManager));

            Vector3I dimensions = chunkManager.GetDimensions();
            float diagonal = (float)Math.Sqrt(Math.Pow(dimensions.X, 2) + Math.Pow(dimensions.Y, 2) + Math.Pow(dimensions.Z, 2));

            while (toBeChecked.Count > 0)
            {
                Chunk currChunk = toBeChecked.Dequeue();
                checkedChunks.Add(currChunk);

                var agents = currChunk.GetAgents();
                var inRangeAgents = agents.Where(IsInRange);
                if (agentFilter != null)
                {
                    inRangeAgents = inRangeAgents.Where(agentFilter);
                }
                result.Agents.AddRange(inRangeAgents);

                var neighbors = chunkManager.GetNeighborChunks(currChunk);
                foreach (var neighbor in neighbors)
                {
                    if (!checkedChunks.Contains(neighbor) && IsChunkIn(neighbor.position * dimensions + (dimensions / 2), diagonal))
                    {
                        toBeChecked.Enqueue(neighbor);
                    }
                }
            }

            return result;
        }

        public ObservationBase OnlyAgent(bool onlyAgent = true)
        {
            this.onlyAgent = onlyAgent;
            return this;
        }

        public ObservationBase SetAgentFilter(Func<IAgent, bool> filter)
        {
            agentFilter = filter;
            return this;
        }
    }
}