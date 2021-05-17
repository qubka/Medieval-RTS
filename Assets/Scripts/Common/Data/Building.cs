using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Building Config", order = 0)]
[Serializable]
public class Building : ScriptableObject
{
	public string description;
	public Sprite icon;
	public Resource[] dependencies;
	//public ProductionType type;
	public float buildingSpeed;
	public int[] cost;
	public InfrastructureType type;
	public BuildingEffect[] effects;
}
/*
[Serializable]
public enum ProductionType
{
	Industrial, 
	Agricultural,
	Military
}*/

[Serializable]
public class BuildingEffect
{
	public BuildingEffectType effect;
	public int[] bonus;

	public string GetName()
	{
		switch (effect) {
			case BuildingEffectType.Loyalty:
				return "Loyalty";
			case BuildingEffectType.Tax:
				return "Tax";
			case BuildingEffectType.Prosperity:
				return "Prosperity";
			case BuildingEffectType.FoodStock:
				return "Food Stock";
			case BuildingEffectType.FoodProduction:
				return "Food Production";
			case BuildingEffectType.GarrisonCapacity:
				return "Garrison Capacity";
			case BuildingEffectType.ArmyRecruitSpeed:
				return "Army Recruit Speed";
			case BuildingEffectType.WallRepairSpeed:
				return "Wall Repair Speed";
			case BuildingEffectType.SiegeEngineSpeed:
				return "Siege Engine Speed";
			case BuildingEffectType.PopulationGrowthSpeed:
				return "Population Growth Speed";
			case BuildingEffectType.VillageDevelopmentDaily:
				return "Village Development Daily";
			case BuildingEffectType.Experience:
				return "Experience";
		}

		return "<Unknown>";
	}
}

[Serializable]
public enum BuildingEffectType
{
	Loyalty,
	Tax,
	Prosperity,
	FoodStock,
	FoodProduction,
	GarrisonCapacity,
	ArmyRecruitSpeed,
	WallRepairSpeed,
	SiegeEngineSpeed,
	PopulationGrowthSpeed,
	VillageDevelopmentDaily,
	Experience
}