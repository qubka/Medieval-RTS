using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityJSON;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Medieval/Character Config", order = 0)]
[Serializable]
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

    [Header("Resources")] 
    public int money;
    public int influence;

    [Header("Game")]
    public CharacterType type;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Settlement home;
    [JSONNode(NodeOptions.DontSerialize)] 
    public List<Settlement> settlements;

    [Header("Data")] 
    [JSONNode(NodeOptions.DontSerialize)] 
    public Faction faction;
    //public string portrait;
    [JSONNode(NodeOptions.DontSerialize)]
    public House house;

    /* For serialization */
    [JSONNode] private int factionId;
    [JSONNode] private int houseId;
    [JSONNode] private int homeId;
    [JSONNode] private int[] settlementsIds;

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
        homeId = home ? home.id : -1;
        settlementsIds = new int[settlements.Count];
        for (var i = 0; i < settlements.Count; i++) {
            settlementsIds[i] = settlements[i].id;
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
        if (homeId != -1) {
            home = game.settlements.Find(s => s.id == homeId);
        }
        settlements.Capacity = settlementsIds.Length;
        foreach (var settlementId in settlementsIds) {
            settlements.Add(game.settlements.Find(s => s.id == settlementId));
        }
        if (settlementsIds.Length > 0) {
            settlementsIds = new int[0];
        }
    }
}

[Serializable]
[JSONEnum(format = JSONEnumMemberFormating.Lowercased)]
public enum CharacterType
{
    Player,
    Noble,
    Bandit,
    Peasant,
}