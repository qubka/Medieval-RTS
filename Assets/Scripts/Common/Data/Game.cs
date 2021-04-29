using System;
using System.Collections.Generic;
using UnityEngine;
using UnityJSON;
using Object = UnityEngine.Object;

[Serializable]
public class Game : ScriptableObject, IDeserializationListener
{
    public List<Faction> factions = new List<Faction>();
    public List<Character> characters = new List<Character>();
    public List<Party> parties = new List<Party>();
    public List<Location> locations = new List<Location>();
    public List<House> houses = new List<House>();
    
    private void OnEnable()
    {
        name = DateTime.Now.ToString("MM_dd_yyyy_h_mm_tt");
    }

    public void Load()
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
        
        var l = Manager.defaultLocations;
        locations.Capacity = l.Count;
        foreach (var location in l) {
            locations.Add(Instantiate(location));
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
            for (var i = 0; i < character.locationsOwned.Count; i++) {
                character.locationsOwned[i] = locations.Find(f => f.id == character.locationsOwned[i].id);
            }
        }

        foreach (var party in parties) {
            if (party.leader) party.leader = characters.Find(c => c.id == party.leader.id);
        }

        foreach (var location in locations) {
            if (location.faction) location.faction = factions.Find(f => f.id == location.faction.id);
            if (location.ruler) location.ruler = characters.Find(c => c.id == location.ruler.id);
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
    }

    public void OnDeserializationFailed(Deserializer deserializer)
    {
    }
}
