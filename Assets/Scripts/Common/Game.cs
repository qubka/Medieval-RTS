using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    [ReadOnly] public List<Faction> factions = new List<Faction>();
    [ReadOnly] public List<Character> characters = new List<Character>();
    [ReadOnly] public List<Party> parties = new List<Party>();
    [ReadOnly] public List<Settlement> settlements = new List<Settlement>();
    [ReadOnly] public List<House> houses = new List<House>();

    protected override void Awake()
    {
        base.Awake();
        
        //load world if it exists
        var save = SaveLoadManager.GetGameData();
        if (save != null) {
            LoadGame(save);
        } else {
            CreateWorld();
        }
    }

    private void Start()
    {
        GenerateWorld();
    }

    public void LoadGame(ProgressSave save) 
    {
        timeController.Load(save.time);
        cameraController.Load(save.camera);

        factions.Capacity = save.factions.Count;
        foreach (var faction in save.factions) {
            factions.Add(Faction.Create(faction));
        }
        
        characters.Capacity = save.characters.Count;
        foreach (var character in save.characters) {
            characters.Add(Character.Create(character));
        }
        
        parties.Capacity = save.parties.Count;
        foreach (var party in save.parties) {
            parties.Add(Party.Create(party));
        }
        
        settlements.Capacity = save.settlements.Count;
        foreach (var settlement in save.settlements) {
            settlements.Add(Settlement.Create(settlement));
        }
        
        houses.Capacity = save.houses.Count;
        foreach (var house in save.houses) {
            houses.Add(House.Create(house));
        }

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
        var defaultFactions = Resources.LoadAll<Faction>("Factions/");
        factions.Capacity = defaultFactions.Length;
        foreach (var faction in defaultFactions) {
            factions.Add(faction.Clone());
        }
        
        var defaultCharacters = Resources.LoadAll<Character>("Characters/");
        characters.Capacity = defaultCharacters.Length;
        foreach (var character in defaultCharacters) {
            characters.Add(character.Clone());
        }
        
        var defaultParties = Resources.LoadAll<Party>("Parties/");
        parties.Capacity = defaultParties.Length;
        foreach (var party in defaultParties) {
            parties.Add(party.Clone());
        }
        
        var defaultSettlements = Resources.LoadAll<Settlement>("Settlements/");
        settlements.Capacity = defaultSettlements.Length;
        foreach (var settlement in defaultSettlements) {
            settlements.Add(settlement.Clone());
        }
        
        var defaultHouses = Resources.LoadAll<House>("Houses/");
        houses.Capacity = defaultHouses.Length;
        foreach (var house in defaultHouses) {
            houses.Add(house.Clone());
        }

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
            if (party.leader.IsPlayer) {
                Player = party;
            }
        }
    }
}
