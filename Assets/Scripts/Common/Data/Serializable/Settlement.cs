using System;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityJSON;
using Wintellect.PowerCollections;

[CreateAssetMenu(menuName = "Medieval/Location Config", order = 0)]
[Serializable]
public class Settlement : SerializableObject
{
    [Header("General")]
    public int id;
    public string label;
    public bool isCapital;
    public bool isMarker;
    public int prosperity;
    public int population;
    public int loyalty;
    public int food;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Character ruler;
    public InfrastructureType type;
    //public List<Troop> garrison;
    //public List<Party> armies;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Settlement[] neighbours;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Resource[] resources;
    [JSONNode(NodeOptions.DontSerialize)] 
    public BuildingDictionary buildings;

    #region Serialization

    [JSONNode] private int rulerId;
    [JSONNode] private Pair<string, int>[] buildingsAssets;

    public override void OnSerialization()
    {
        rulerId = ruler ? ruler.id : -1;
        var i = 0;
        buildingsAssets = new Pair<string, int>[buildings.Count];
        foreach (var pair in buildings) {
            buildingsAssets[i] = new Pair<string, int>(Path.GetFileName(AssetDatabase.GetAssetPath(pair.Key)), pair.Value);
            i++;
        }
    }

    public override void OnDeserialization()
    {
        var game = SaveLoadManager.Instance.current;
        if (rulerId != -1) {
            ruler = game.characters.Find(c => c.id == rulerId);
        }
        foreach (var building in buildingsAssets) {
            buildings.Add(AssetDatabase.LoadAssetAtPath<Building>("Assets/Resources/Buildings/" + type + "/" + building.First), building.Second);
        }
        if (buildingsAssets.Length > 0) {
            buildingsAssets = new Pair<string, int>[0];
        }
        var settlement = Manager.defaultSettlements.Find(s => s.id == id);
        resources = settlement.resources;
        neighbours = settlement.neighbours;
    }
    
    #endregion
}

[Serializable]
[JSONEnum(format = JSONEnumMemberFormating.Lowercased)]
public enum InfrastructureType
{
    City,
    Castle,
    Village
}

[Serializable]
public class BuildingDictionary : SerializableDictionary<Building, int>
{
}