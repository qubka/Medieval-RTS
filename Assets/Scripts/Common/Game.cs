using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityJSON;

[Serializable]
public class Game : SingletonObject<Game>, IDeserializationListener
{
    public static Party Player { get; private set; } = Party.Dummy();
    public static DateTime Now => Instance.dateTime;
    public static List<Faction> Factions => Instance.factions;
    public static List<Character> Characters => Instance.characters;
    public static List<Party> Parties => Instance.parties;
    public static List<Settlement> Settlements => Instance.settlements;
    public static List<House> Houses => Instance.houses;
    
    /* Serialization */
    
    [JSONNode, SerializeField] private List<Faction> factions = new List<Faction>();
    [JSONNode, SerializeField] private List<Character> characters = new List<Character>();
    [JSONNode, SerializeField] private List<Party> parties = new List<Party>();
    [JSONNode, SerializeField] private List<Settlement> settlements = new List<Settlement>();
    [JSONNode, SerializeField] private List<House> houses = new List<House>();
    [JSONNode] private DateTime dateTime = new DateTime(1080, 1, 1, 12, 0, 0);
    [JSONNode] private int prevDay = 1;
    
    private void Start()
    {
        NewGame();
        OnDeserializationSucceeded(null);
        StartCoroutine(Tick());
    }

    private IEnumerator Tick()
    {
        while (true) {
            OnUpdate();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnUpdate()
    {
        dateTime = dateTime.AddMinutes(1);
        var day = dateTime.Day;
        if (day != prevDay) {
            var events = Events.Instance;
            events.OnDailyTickEvent();
            if (dateTime.DayOfWeek == DayOfWeek.Monday) {
                events.OnWeeklyTickEvent();
            }
        }
        prevDay = day;
    }

    public void NewGame()
    {
        var f = Manager.defaultFactions;
        factions.Capacity = f.Count;
        foreach (var faction in f) {
            factions.Add(Instantiate(faction));
        }
        
        var c = Manager.defaultCharacters;
        characters.Capacity = c.Count;
        foreach (var character in c) {
            characters.Add(Instantiate(character));
        }
        
        var p = Manager.defaultParties;
        parties.Capacity = p.Count;
        foreach (var party in p) {
            parties.Add(Instantiate(party));
        }
        
        var s = Manager.defaultSettlements;
        settlements.Capacity = s.Count;
        foreach (var settlement in s) {
            settlements.Add(Instantiate(settlement));
        }
        
        var h = Manager.defaultHouses;
        houses.Capacity = h.Count;
        foreach (var house in h) {
            houses.Add(Instantiate(house));
        }

        // Adjust data to the copies of scriptable objects instead of real ones

        foreach (var faction in factions) {
            if (faction.leader) faction.leader = characters.Find(c => c.id == faction.leader.id);
            for (var i = 0; i < faction.allies.Count; i++) {
                faction.allies[i] = factions.Find(f => f.id == faction.allies[i].id);
            }
            for (var i = 0; i < faction.enemies.Count; i++) {
                faction.enemies[i] = factions.Find(f => f.id == faction.enemies[i].id);
            }
        }

        foreach (var character in characters) {
            if (character.faction) character.faction = factions.Find(f => f.id == character.faction.id);
            //if (character.party) character.party = parties.Find(p => p.leader.id == character.party.leader);
            for (var i = 0; i < character.settlements.Count; i++) {
                character.settlements[i] = settlements.Find(s => s.id == character.settlements[i].id);
            }
        }

        foreach (var party in parties) {
            if (party.leader) party.leader = characters.Find(c => c.id == party.leader.id);
        }

        foreach (var settlement in settlements) {
            if (settlement.ruler) settlement.ruler = characters.Find(c => c.id == settlement.ruler.id);
            for (var i = 0; i < settlement.neighbours.Length; i++) {
                settlement.neighbours[i] = settlements.Find(s => s.id == settlement.neighbours[i].id);
            }
        }
    }

    public void OnDeserializationWillBegin(Deserializer deserializer)
    {
    }

    public void OnDeserializationSucceeded(Deserializer deserializer)
    {
        foreach (var party in parties) {
            var army = Instantiate(Manager.global.armyPrefab, party.position, party.rotation).GetComponent<Army>();
            army.data = party;
            if (party.leader.IsPlayer) {
                Player = party;
            }
        }

        foreach (var settlement in settlements) {
            var town = TownTable.Instance.Values.First(t => t.GetID() == settlement.id);
            town.data = settlement;
            settlement.data = town;
        }
    }

    public void OnDeserializationFailed(Deserializer deserializer)
    {
    }
}
