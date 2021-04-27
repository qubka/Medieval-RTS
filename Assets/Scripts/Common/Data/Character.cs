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
    public string surname;
    public string title;
    public Gender gender;
    public int age;
    public int renown;
    public int honor;

    [Header("Game")] 
    public bool isNoble;
    public bool isPlayer;
    //public bool isCompanion;
    [JSONNode(NodeOptions.DontSerialize)] 
    public List<Location> locationsOwned;

    [Header("Data")] 
    [JSONNode(NodeOptions.DontSerialize)] 
    public Faction faction;
    //public string portrait;
    [JSONNode(NodeOptions.DontSerialize)]
    public Banner banner;

    [JSONNode] 
    private string factionName;
    [JSONNode] 
    private string[] locationsNames;
    [JSONNode] 
    private string bannerName;
    
#if UNITY_EDITOR    
    public void GenerateName(CharacterNames names)
    {
        var instance = GetInstanceID();
        var newName = names.RandomName;
        var assetPath = AssetDatabase.GetAssetPath(instance);
        AssetDatabase.RenameAsset(assetPath, newName + instance);
        name = newName;
        surname = newName;
        age = Random.Range(17, 50);
        renown = Random.Range(0, 1000);
        honor = Random.Range(-100, 100);
    }
#endif
    
    public void OnEnable()
    {
        hash = surname.GetHashCode();
    }

    public override void OnSerialization()
    {
        factionName = faction.label;
        locationsNames = new string[locationsOwned.Count];
        for (var i = 0; i < locationsOwned.Count; i++) {
            locationsNames[i] = locationsOwned[i].label;
        }
        bannerName = banner ? Path.GetFileName(AssetDatabase.GetAssetPath(banner)) : "";
    }

    public override void OnDeserialization()
    {
        var game = SaveLoadManager.Instance.current;
        faction = game.factions.Find(f => f.label == factionName);
        locationsOwned.Capacity = locationsNames.Length;
        foreach (var locationName in locationsNames) {
            locationsOwned.Add(game.locations.Find(l => l.name == locationName));
        }
        if (bannerName.Length > 0) {
            banner = AssetDatabase.LoadAssetAtPath<Banner>("Assets/Resources/Banners/" + bannerName);
        }
    }
}   