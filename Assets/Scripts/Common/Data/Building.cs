using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Medieval/Building Config", order = 0)]
[Serializable]
public class Building : ScriptableObject
{
	public string label;
	public Sprite icon;
	public Resource[] dependencies;
	public ProductionType type;
}

[Serializable]
public enum ProductionType
{
	Industrial, 
	Agricultural,
	Military
}