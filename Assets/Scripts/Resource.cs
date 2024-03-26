using System;

public class Resource
{
	// Later I'll add more resource types, currently just wood and gold
	public enum ResourceType
	{
		Gold, Wood
	}

	public ResourceType type;
	public int amount;

	public Resource(ResourceType type, int amount)
	{
		this.type = type;
		this.amount = amount;
	}
}
