using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityJSON;
using Object = UnityEngine.Object;

[Serializable]
public class Game : SingletonObject<Game>, IDeserializationListener
{
    public List<Faction> factions = new List<Faction>();
    public List<Character> characters = new List<Character>();
    public List<Party> parties = new List<Party>();
    public List<Settlement> settlements = new List<Settlement>();
    public List<House> houses = new List<House>();
    
    private void Start()
    {
        NewGame();
        OnDeserializationSucceeded(null);
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
        }
    }

    public void OnDeserializationWillBegin(Deserializer deserializer)
    {
    }

    public void OnDeserializationSucceeded(Deserializer deserializer)
    {
        foreach (var party in parties) {
            Instantiate(Manager.global.armyPrefab, party.position, party.rotation).GetComponent<Army>().data = party;
        }

        foreach (var settlement in settlements) {
            TownTable.Instance.Values.First(t => t.data.id == settlement.id).data = settlement;
        }
    }

    public void OnDeserializationFailed(Deserializer deserializer)
    {
    }
}
