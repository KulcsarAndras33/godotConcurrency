using System;
using System.Collections.Generic;

public class Storage
{
    private Dictionary<string, int> resources = new();
    private int capacity;
    private int currentAmount = 0;

    public Storage(int capacity)
    {
        this.capacity = capacity;
    }

    public bool TryStore(string resource, int amount)
    {
        if (currentAmount + amount > capacity)
        {
            return false;
        }

        if (!resources.ContainsKey(resource))
        {
            resources[resource] = 0;
        }

        resources[resource] += amount;
        currentAmount += amount;
        return true;
    }

    public bool TryRetrieve(string resource, int amount)
    {
        if (!resources.ContainsKey(resource) || resources[resource] < amount)
        {
            return false;
        }

        resources[resource] -= amount;
        currentAmount -= amount;
        return true;
    }

    public void ChangeCapacity(int newCapacity)
    {
        capacity = newCapacity;
    }

    public int GetResourceAmount(string resource)
    {
        return resources.ContainsKey(resource) ? resources[resource] : 0;
    }
}