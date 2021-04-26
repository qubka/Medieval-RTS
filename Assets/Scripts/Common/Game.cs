using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Game : SingletonObject<Game>
{
    public GameObject army;
    public List<Faction> factions = new List<Faction>();

    public void Awake()
    {
        var f = Resources.LoadAll<Faction>("Factions/");
        factions.Capacity = f.Length;
        foreach (var prototype in f) {
            var faction = Instantiate(prototype);
            faction.name = prototype.name;
            faction.characters = Resources.LoadAll<Character>("Characters/").ToList().Where(c => c.faction == prototype).ToList();
            faction.locations = Resources.LoadAll<Location>("Locations/").ToList().Where(l => l.faction == prototype).ToList();
            faction.parties = Resources.LoadAll<Party>("Parties/").ToList().Where(p => p.leader.faction == prototype).ToList();
            factions.Add(faction);

            foreach (var party in faction.parties) {
                Instantiate(army, party.position, party.rotation).GetComponent<Army>().data = party;
            }
        }
    }
}
