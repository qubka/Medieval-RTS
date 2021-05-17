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
    [JSONNode(NodeOptions.DontSerialize)] 
    public List<Troop> garrison;
    [JSONNode(NodeOptions.DontSerialize)] 
    public List<Party> parties;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Settlement[] neighbours;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Resource[] resources;
    [JSONNode(NodeOptions.DontSerialize)] 
    public BuildingDictionary buildings;

    #region Serialization

    [JSONNode] private int rulerId;
    [JSONNode] private int[] partiesIds;
    [JSONNode] private Pack<int, int>[] garrisonData;
    [JSONNode] private Pack<string, Pack<int, float>>[] buildingsAssets;
    
    public override void OnSerialization()
    {
        rulerId = ruler ? ruler.id : -1;
        garrisonData = new Pack<int, int>[garrison.Count];
        for (var i = 0; i < garrison.Count; i++) {
            var troop = garrison[i];
            garrisonData[i] = new Pack<int, int>(Array.IndexOf(ruler.faction.troops, troop), troop.size);
        }
        partiesIds = new int[parties.Count];
        for (var i = 0; i < parties.Count; i++) {
            partiesIds[i] = parties[i].leader.id;
        }
        var x = 0;
        buildingsAssets = new Pack<string, Pack<int, float>>[buildings.Count];
        foreach (var pair in buildings) {
            buildingsAssets[x] = new Pack<string, Pack<int, float>>(Path.GetFileName(AssetDatabase.GetAssetPath(pair.Key)), pair.Value);
            x++;
        }
        
    }

    public override void OnDeserialization()
    {
        var game = SaveLoadManager.Instance.current;
        if (rulerId != -1) {
            ruler = game.characters.Find(c => c.id == rulerId);
        }
        garrison.Capacity = garrisonData.Length;
        foreach (var pack in garrisonData) {
            var troop = ruler.faction.troops[pack.item1];
            troop.size = pack.item2;
            garrison.Add(troop);
        }
        if (garrisonData.Length > 0) {
            garrisonData = new Pack<int, int>[0];
        }
        parties.Capacity = partiesIds.Length;
        foreach (var leaderId in partiesIds) {
            parties.Add(game.parties.Find(p => p.leader.id == leaderId));
        }
        if (partiesIds.Length > 0) {
            partiesIds = new int[0];
        }
        foreach (var building in buildingsAssets) {
            buildings.Add(AssetDatabase.LoadAssetAtPath<Building>("Assets/Resources/Buildings/" + type + "/" + building.item1), building.item2);
        }
        if (buildingsAssets.Length > 0) {
            buildingsAssets = new Pack<string, Pack<int, float>>[0];
        }
        var settlement = Manager.defaultSettlements.Find(s => s.id == id);
        resources = settlement.resources;
        neighbours = settlement.neighbours;
    }
    
    #endregion
    
    public void Build(Building building)
    {
        if (buildings.TryGetValue(building, out var data)) {
            if (data.item2 < 1f) {
                data.item1--;
                data.item2 = 1f;
                if (data.item1 < 0) {
                    buildings.Remove(building);
                }
            } else {
                data.item1++;
                data.item2 = Manager.Instance.isCheat ? 1f : 0f;
            }
        } else {
            buildings.Add(building, new Pack<int, float>(0, Manager.Instance.isCheat ? 1f : 0f));
        }
    }
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
public class BuildingDictionary : SerializableDictionary<Building, Pack<int, float>>
{
}