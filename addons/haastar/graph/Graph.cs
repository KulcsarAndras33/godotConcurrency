using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Graph : GodotObject
{
	Dictionary<long, List<Edge>> edges = new Dictionary<long, List<Edge>>();
	Dictionary<long, Vector3I> vertices = new Dictionary<long, Vector3I>();

	public void AddVertex(long id, Vector3I position)
	{
		vertices[id] = position;
		edges[id] = [];
	}

	// Bidirectional edge by default
	public void AddEdge(long from, long to, int weight)
	{
		AddDirectedEdge(from, to, weight);
		AddDirectedEdge(to, from, weight);
	}

	public void AddEdge(long from, long to, int weight, bool bidirectional = true)
	{
		AddDirectedEdge(from, to, weight);
		if (bidirectional)
		{
			AddDirectedEdge(to, from, weight);
		}
	}

	public void AddDirectedEdge(long from, long to, int weight)
	{
		/* Skip this check for now
		if (!edges.ContainsKey(from) || !edges.ContainsKey(to))
		{
			GD.PrintErr("One or both vertices not found in graph.");
			return;
		}
		*/

		Edge edge = new Edge
		{
			to = to,
			weight = weight
		};
		edges[from].Add(edge);
	}

	public long GetClosestVertexId(Vector3I position)
	{
		long closestId = -1;
		float closestDistance = float.MaxValue;

		foreach (var vertex in vertices)
		{
			float distance = vertex.Value.DistanceTo(position);
			if (distance < closestDistance)
			{
				closestDistance = distance;
				closestId = vertex.Key;
			}
		}

		return closestId;
	}

	public Vector3[] GetPath(long startId, long endId)
	{
		/*
		 * 
		GD.Print("Abstract vertex count: " + vertices.Count);
		var edgeCount = 0;
		var weightSum = 0;
		var distSum = 0;
		var enumerator = edges.GetEnumerator();
		var rand = new Random();
		for (int i = 0; i < 500; i++)
		{
			enumerator.MoveNext();
			var count = enumerator.Current.Value.Count;
			if (count > 1)
			{
				edgeCount += count;
				var idx = rand.Next(count);
				weightSum += enumerator.Current.Value[idx].weight;
				distSum += (int)vertices[enumerator.Current.Key].DistanceTo(vertices[enumerator.Current.Value[idx].to]);
			}
		}
		GD.Print("Abstract avg edge count: " + edgeCount / 200);
		GD.Print("Abstract avg weight: " + weightSum / 200);
		GD.Print("Abstract avg distance: " + distSum / 200);

		*/
        int vertexChecks = 0;
		var watch = System.Diagnostics.Stopwatch.StartNew();
		var path = new List<Vector3>();
		var cameFrom = new Dictionary<long, long>();
		var costSoFar = new Dictionary<long, int>();
		var priorityQueue = new PriorityQueue<long, int>();

		priorityQueue.Enqueue(startId, 0);
		cameFrom[startId] = startId;
		costSoFar[startId] = 0;

		while (priorityQueue.Count > 0)
		{
			long current = priorityQueue.Dequeue();

			if (current == endId)
			{
				break;
			}

			foreach (var edge in edges[current])
			{
				vertexChecks++;
				int newCost = costSoFar[current] + edge.weight;
				if (!costSoFar.ContainsKey(edge.to) || newCost < costSoFar[edge.to])
				{
					costSoFar[edge.to] = newCost;
					var a = vertices[edge.to];
					var b = vertices[endId];
					int priority = (int)(newCost +  (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) * 1.3);
					priorityQueue.Enqueue(edge.to, priority); // TODO There can be multiple entries for the same node. Is that a problem?
					cameFrom[edge.to] = current;
				}
			}
		}

		if (!cameFrom.ContainsKey(endId))
		{
			return []; // No path found
		}

		long temp = endId;
		while (temp != startId)
		{
			path.Add(vertices[temp]);
			temp = cameFrom[temp];
		}
		path.Add(vertices[startId]);
		path.Reverse();

		watch.Stop();
		GD.Print("Vertex checks: " + vertexChecks);
		GD.Print("Pathfinding took " + watch.ElapsedMilliseconds);
		return path.ToArray();
	}
	
	public bool HasVertex(long id)
	{
		return vertices.ContainsKey(id);
	}
}
