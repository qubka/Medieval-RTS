using System;
using System.Collections.Generic;
using UnityEngine;
using UnityJSON;
using Object = UnityEngine.Object;

[Serializable]
public class Game : ScriptableObject
{
    public List<Faction> factions = new List<Faction>();
    public List<Character> characters = new List<Character>();
    public List<Party> parties = new List<Party>();
    public List<Location> locations = new List<Location>();

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

        // Adjast data to the copies of scriptable objects instead of real ones

        foreach (var faction in factions) {
            if (faction.leader) faction.leader = characters.Find(c => c & faction.leader);
            for (var i = 0; i < faction.allies.Count; i++) {
                faction.allies[i] = factions.Find(f => f & faction.allies[i]);
            }
            for (var i = 0; i < faction.enemies.Count; i++) {
                faction.enemies[i] = factions.Find(f => f & faction.enemies[i]);
            }
        }

        foreach (var character in characters) {
            if (character.faction) character.faction = factions.Find(f => f & character.faction);
            //if (character.party) character.party = parties.Find(p => p.leader & character.party.leader);
            for (var i = 0; i < character.locationsOwned.Count; i++) {
                character.locationsOwned[i] = locations.Find(f => f & character.locationsOwned[i]);
            }
        }

        foreach (var party in parties) {
            if (party.leader) party.leader = characters.Find(c => c & party.leader);
            //Object.Instantiate(Manager.global.armyPrefab, party.position, party.rotation).GetComponent<Army>().data = party;
        }

        foreach (var location in locations) {
            if (location.faction) location.faction = factions.Find(f => f & location.faction);
            if (location.ruler) location.ruler = characters.Find(c => c & location.ruler);
        }
    }

    public void Load(string json)
    {
        throw new NotImplementedException();
    }

    public string Save()
    {
        //var database = new string[3];
        // database[0] = ToJSONString(factions);
        //database[1] = ToJSONString(characters);
        //database[2] = ToJSONString(parties);
        //database[0] = ToJSONString(locations);
        return characters.ToJSONString();
    }
}
