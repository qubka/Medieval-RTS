using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Medieval/Building Config", order = 0)]
[Serializable]
public class Building : ScriptableObject
{
	public string label;
	public string description;
	public Sprite icon;
	public Resource[] dependencies;
	//public ProductionType type;
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