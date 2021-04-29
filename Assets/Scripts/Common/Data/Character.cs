using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityJSON;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Medieval/Character Config", order = 0)]
[Serializable]
[InitializeOnLoad]
public class Character : SerializableObject
{
    [Header("Primary")]
    public int id;
    public string surname;
    public string title;
    public Gender gender;
    public int age;
    public int renown;
    public int honor;

    [Header("Game")]
    public bool isPlayer;
    //public bool isCompanion;
    [JSONNode(NodeOptions.DontSerialize)] 
    public List<Location> locationsOwned;

    [Header("Data")] 
    [JSONNode(NodeOptions.DontSerialize)] 
    public Faction faction;
    //public string portrait;
    [JSONNode(NodeOptions.DontSerialize)]
    public House house;

    /* For serialization */
    [JSONNode] private int factionId;
    [JSONNode] private int houseId;
    [JSONNode] private int[] locationsIds;

#if UNITY_EDITOR    
    public void GenerateName(CharacterNames names)
    {
        id = Resources.LoadAll<Character>("Characters/").Length;
        var instance = GetInstanceID();
        var newName = names.RandomName;
        var assetPath = AssetDatabase.GetAssetPath(instance);
        AssetDatabase.RenameAsset(assetPath, newName + "_" + id);
        name = newName;
        surname = newName;
        age = Random.Range(17, 50);
        renown = Random.Range(0, 1000);
        honor = Random.Range(-100, 100);
    }
#endif

    public override void OnSerialization()
    {
        factionId = faction ? faction.id : -1;
        houseId = house ? house.id : -1;
        locationsIds = new int[locationsOwned.Count];
        for (var i = 0; i < locationsOwned.Count; i++) {
            locationsIds[i] = locationsOwned[i].id;
        }
    }

    public override void OnDeserialization()
    {
        var game = SaveLoadManager.Instance.current;
        if (factionId != -1) {
            faction = game.factions.Find(f => f.id == factionId);
        }
        if (houseId != -1) {
            house = game.houses.Find(h => h.id == houseId);
        }
        locationsOwned.Capacity = locationsIds.Length;
        foreach (var locationId in locationsIds) {
            locationsOwned.Add(game.locations.Find(l => l.id == locationId));
        }
        if (locationsIds.Length > 0) {
            locationsIds = new int[0];
        }
    }
}   