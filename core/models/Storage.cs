using System;
using System.Collections.Generic;

public class Storage
{
    private readonly Dictionary<int, float> resources = [];
    private float capacity;
    private float currentAmount = 0;

    public Storage(float capacity)
    {
        this.capacity = capacity;
    }

    public bool TryStore(int resourceId, float amount)
    {
        if (currentAmount + amount > capacity)
        {
            return false;
        }

        if (!resources.ContainsKey(resourceId))
        {
            resources[resourceId] = 0;
        }

        resources[resourceId] += amount;
        currentAmount += amount;
        return true;
    }

    public bool TryRetrieve(int resourceId, float amount)
    {
        if (!resources.ContainsKey(resourceId) || resources[resourceId] < amount)
        {
            return false;
        }

        resources[resourceId] -= amount;
        currentAmount -= amount;
        return true;
    }

    public void ChangeCapacity(int newCapacity)
    {
        capacity = newCapacity;
    }

    public float GetResourceAmount(int resourceId)
    {
        return resources.ContainsKey(resourceId) ? resources[resourceId] : 0;
    }

    public Dictionary<int, float> GetAllResources()
    {
        return resources;
    }
}