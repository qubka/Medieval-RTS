using System;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityJSON;

[CreateAssetMenu(menuName = "Medieval/Location Config", order = 0)]
[Serializable]
public class Location : SerializableObject
{
    [Header("General")]
    public int id;
    public string label;
    public bool isCapital;
    public int population;
    public int production;
    public int loyalty;
    public int food;
    public int militia;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Character ruler;
    public InfrastructureType type;
    //public List<Troop> garrison;
    //public List<Party> armies;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Resource[] localResources;
    [JSONNode(NodeOptions.DontSerialize)] 
    public List<Building> buildings = new List<Building>();

    [JSONNode] private int rulerId;
    [JSONNode] private string[] buildingsAssets;

    public override void OnSerialization()
    {
        rulerId = ruler ? ruler.id : -1;
        buildingsAssets = new string[buildings.Count];
        for (var i = 0; i < buildings.Count; i++) {
            buildingsAssets[i] = Path.GetFileName(AssetDatabase.GetAssetPath(buildings[i]));
        }
    }

    public override void OnDeserialization()
    {
        var game = SaveLoadManager.Instance.current;
        if (rulerId != -1) {
            ruler = game.characters.Find(c => c.id == rulerId);
        }
        foreach (var building in buildingsAssets) {
            buildings.Add(AssetDatabase.LoadAssetAtPath<Building>("Assets/Resources/Buildings/" + building));
        }
        var location = Manager.defaultLocations.Find(f => f.id == id);
        localResources = location.localResources;
    }
}

[Serializable]
[JSONEnum(format = JSONEnumMemberFormating.Lowercased)]
public enum InfrastructureType
{
    Town,
    Castle,
    Village
}