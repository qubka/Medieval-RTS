using System;
using System.Collections.Generic;
using System.Linq;
using Den.Tools;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Medieval/Templates/Character", order = 0)]
[Serializable]
public class Character : ScriptableObject
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
    public Settlement home;
    public List<Settlement> settlements = new List<Settlement>();

    [Header("Data")] 
    public Faction faction;
    //public string portrait;
    public House house;

    public static List<Character> All => Game.Characters;
    public bool IsPlayer => type == CharacterType.Player;
    public bool IsPeasant => type == CharacterType.Peasant;
    public bool IsBandit => type == CharacterType.Bandit;
    public bool IsNoble => type == CharacterType.Noble;
    
    #region Serialization

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

    /*public override void OnSerialization()
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
        
        settlements.Capacity = settlementsIds.Length;
        foreach (var settlementId in settlementsIds) {
            settlements.Add(Game.Settlements.Find(s => s.id == settlementId));
        }
        if (settlementsIds.Length > 0) {
            settlementsIds = new int[0];
        }
    }*/
    
    #endregion

    public static Character Create(CharacterSave save)
    {
        var obj = CreateInstance<Character>();
        obj.id = save.id;
        obj.name = save.name;
        return obj;
    }

    public void Load(CharacterSave save = null)
    {
        if (save != null) {
            surname = save.surname;
            title = save.title;
            gender = (Gender) save.gender;
            age = save.age;
            renown = save.renown;
            honor = save.honor;
            money = save.money;
            influence = save.influence;
            type = (CharacterType) save.type;
            if (save.home != -1) home = Settlement.All.First(s => s.id == save.home);
            if (save.faction != -1) faction = Faction.All.First(f => f.id == save.faction);
            if (save.house != -1) house = House.All.First(h => h.id == save.house);
            settlements = Settlement.All.Where(s => save.settlements.Contains(s.id)).ToList();
            
        } else {
            if (home) home = Settlement.All.First(s => s.id == home.id);
            if (faction) faction = Faction.All.First(f => f.id == faction.id);
            if (house) house = House.All.First(h => h.id == house.id);
            for (var i = 0; i < settlements.Count; i++) {
                settlements[i] = Settlement.All.First(s => s.id == settlements[i].id);
            }
        }
    }

    public Character Clone()
    {
        var obj = Instantiate(this);
        obj.name = obj.name.Replace("(Clone)", "");
        return obj;
    }
}

[Serializable]
public enum CharacterType
{
    Player,
    Noble,
    Bandit,
    Peasant,
}

[Serializable]
public class CharacterSave
{
    [HideInInspector] public int id;
    [HideInInspector] public string name;
    [HideInInspector] public string surname;
    [HideInInspector] public string title;
    [HideInInspector] public int gender;
    [HideInInspector] public int age;
    [HideInInspector] public int renown;
    [HideInInspector] public int honor;
    [HideInInspector] public int money;
    [HideInInspector] public int influence;
    [HideInInspector] public int type;
    [HideInInspector] public int home;
    [HideInInspector] public int faction;
    [HideInInspector] public int house;
    [HideInInspector] public int[] settlements;
    
    public CharacterSave(Character character)
    {
        id = character.id;
        name = character.name;
        surname = character.surname;
        title = character.title;
        gender = (int) character.gender;
        age = character.age;
        renown = character.renown;
        honor = character.honor;
        money = character.money;
        influence = character.influence;
        type = (int) character.type;
        home = character.home ? character.home.id : -1;
        faction = character.faction ? character.faction.id : -1;
        house = character.house ? character.house.id : -1;
        settlements = character.settlements.Select(s => s.id).ToArray();
    }
}