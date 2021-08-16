using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Den.Tools;
using UnityEngine;

[Serializable]
public class Game : SingletonObject<Game>
{
    [Header("Refs")] 
    public TimeController timeController;
    public CameraController cameraController;
    
    public static Party Player { get; private set; }
    public static List<Faction> Factions => Instance.factions;
    public static List<Character> Characters => Instance.characters;
    public static List<Party> Parties => Instance.parties;
    public static List<Settlement> Settlements => Instance.settlements;
    public static List<House> Houses => Instance.houses;
    
    /* Serialization */
    
    [ReadOnly] public List<Faction> factions;
    [ReadOnly] public List<Character> characters;
    [ReadOnly] public List<Party> parties;
    [ReadOnly] public List<Settlement> settlements;
    [ReadOnly] public List<House> houses;

    protected override void Awake()
    {
        base.Awake();
        
        //load world if it exists
        var save = SaveLoadManager.GetGameData();
        if (save != null) {
            LoadWorld(save);
        } else {
            CreateWorld();
        }
        
        Player = parties.FirstOrDefault(p => p.leader.IsPlayer);
    }

    private void Start()
    {
        GenerateWorld();
    }

    public void LoadWorld(ProgressSave save) 
    {
        timeController.Load(save.time);
        cameraController.Load(save.camera);

        // Initialization
        factions = save.factions.Select(Faction.Create).ToList();
        characters = save.characters.Select(Character.Create).ToList();
        parties = save.parties.Select(Party.Create).ToList();
        settlements = save.settlements.Select(Settlement.Create).ToList();
        houses = save.houses.Select(House.Create).ToList();

        // Loading
        for (var i = 0; i < factions.Count; i++) {
            factions[i].Load(save.factions[i]);
        }
        for (var i = 0; i < characters.Count; i++) {
            characters[i].Load(save.characters[i]);
        }
        for (var i = 0; i < parties.Count; i++) {
            parties[i].Load(save.parties[i]);
        }
        for (var i = 0; i < settlements.Count; i++) {
            settlements[i].Load(save.settlements[i]);
        }
        for (var i = 0; i < houses.Count; i++) {
            houses[i].Load(save.houses[i]);
        }
    }
    
    public void CreateWorld()
    {
        // Initialization
        factions = Resources.LoadAll<Faction>("Factions/").Select(Faction.Copy).ToList();
        characters =  Resources.LoadAll<Character>("Characters/").Select(Character.Copy).ToList();
        parties = Resources.LoadAll<Party>("Parties/").Select(Party.Copy).ToList();
        settlements = Resources.LoadAll<Settlement>("Settlements/").Select(Settlement.Copy).ToList();
        houses = Resources.LoadAll<House>("Houses/").Select(House.Copy).ToList();

        // Loading
        foreach (var faction in factions) {
            faction.Load();
        }
        foreach (var character in characters) {
            character.Load();
        }
        foreach (var party in parties) {
            party.Load();
        }
        foreach (var settlement in settlements) {
            settlement.Load();
        }
        foreach (var house in houses) {
            house.Load();
        }
    }

    public void GenerateWorld()
    {
        foreach (var party in parties) {
            var army = Instantiate(Manager.global.armyPrefab, party.position, party.rotation).GetComponent<Army>();
            army.data = party;
        }
    }
}
