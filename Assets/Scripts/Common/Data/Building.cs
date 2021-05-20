using System;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Building Config", order = 0)]
[Serializable]
public class Building : ScriptableObject
{
	public string description;
	public Sprite icon;
	public Resource[] dependencies;
	//public Resource[] production;
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
}

[Serializable]
public enum BuildingEffectType
{
	Loyalty,
	Tax,
	Prosperity,
	[Description("Food Stock")]
	FoodStock,
	[Description("Food Production")]
	FoodProduction,
	[Description("Garrison Capacity")]
	GarrisonCapacity,
	[Description("Army Recruit Speed")]
	ArmyRecruitSpeed,
	[Description("Wall Repair Speed")]
	WallRepairSpeed,
	[Description("Siege Engine Speed")]
	SiegeEngineSpeed,
	[Description("Population Growth Speed")]
	PopulationGrowthSpeed,
	[Description("Village Development Daily")]
	VillageDevelopmentDaily,
	Experience,
	[Description("Armor Quality Bonus")]
	ArmorQualityBonus,
	[Description("Weapon Quality Bonus")]
	WeaponQualityBonus,
	Trade,
	Gold,
	[Description("Army Movement Speed")]
	ArmyMovementSpeed,
	[Description("Research Cost Reduction")]
	ResearchCostReduction,
	[Description("Influence Growth Speed")]
	InfluenceGrowthSpeed,
	[Description("Army Upkeep Cost")]
	ArmyUpkeepCost
}