using System;
using System.Collections.Generic;
using core.models.descriptor;
using Godot;

public class Storage
{
    private static readonly Library<ResourceDescriptor> resourceLibrary = Library<ResourceDescriptor>.GetInstance();
    private readonly Dictionary<int, float> resources = [];
    private readonly Dictionary<string, float> resourcesByTag = [];
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
        foreach (var tag in resourceLibrary.GetDescriptorById(resourceId).Tags)
        {
            if (!resourcesByTag.ContainsKey(tag))
            {
                resourcesByTag[tag] = 0;
            }
            resourcesByTag[tag] += amount;
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

    public bool TryRetrieve(string tag, float amount)
    {
        if (!resourcesByTag.ContainsKey(tag) || resourcesByTag[tag] < amount)
        {
            return false;
        }

        currentAmount -= amount;

        var resourceEnum = resources.Keys.GetEnumerator();

        while (amount > 0 && resourceEnum.MoveNext())
        {
            var amountToRetrieve = Math.Min(amount, resources[resourceEnum.Current]);
            resources[resourceEnum.Current] -= amountToRetrieve;
            amount -= amountToRetrieve;
        }

        if (amount > 0)
        {
            throw new Exception($"Remaining amount is {amount} after retrieving by tag {tag}. Something went wrong!");
        }

        return true;
    }

    public void ChangeCapacity(int newCapacity)
    {
        capacity = newCapacity;
    }

    public float GetResourceAmount(int resourceId)
    {
        return resources.TryGetValue(resourceId, out float value) ? value : 0;
    }

    public float GetResourceAmount(string tag)
    {
        return resourcesByTag.TryGetValue(tag, out float value) ? value : 0;
    }

    public Dictionary<int, float> GetAllResources()
    {
        return resources;
    }
}