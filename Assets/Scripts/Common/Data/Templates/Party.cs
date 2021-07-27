using System;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Medieval/Templates/Party", order = 0)]
[Serializable]
public class Party : ScriptableObject
{ 
    public Character leader;
    public float morale;
    public float speed;
    public Vector3 position;
    public Quaternion rotation; 
    public List<Troop> troops = new List<Troop>();
    public Settlement localSettlement;
    public Settlement targetSettlement; // for AI
    public Party enemyParty;
    public ExternalBehavior behavior;
    public int skin;

    public static List<Party> All => Game.Parties;
    public int TroopStrength => troops.Sum(t => t.size * t.data.TotalStats);
    public int TroopCount => troops.Sum(t => t.size);
    public int TroopWage => -Convert.ToInt32(troops.Sum(t => t.data.recruitCost * ((float) t.size / t.data.maxCount)));

    public static void CreatePeasant(Settlement settlement)
    {
        var leader = CreateInstance<Character>();
        leader.name = "Peasant Elder";
        leader.id = Character.All.OrderByDescending(c => c.id).First().id++;
        leader.faction = settlement.ruler.faction;
        leader.type = CharacterType.Peasant;
        leader.home = settlement;
        
        var party = CreateInstance<Party>();
        party.name = "Peasants";
        party.leader = leader;
        party.skin = Random.Range(0, 2);
        party.targetSettlement = settlement.neighbours[0];
        party.behavior = Manager.global.behavior;
        
        var count = math.min(math.max(1, settlement.prosperity / 10), 3);
        var troops = Manager.global.troops;
        for (var i = 0; i < count; i++) {
            party.troops.Add(troops[Random.Range(0, troops.Length)].Clone());
        }
        
        Party.All.Add(party);
        Character.All.Add(leader);

        var town = settlement.data;
        var army = Instantiate(Manager.global.armyPrefab, town.doorPosition, town.doorRotation).GetComponent<Army>();
        army.data = party;
    }

    public static void CreateBandit()
    {
        // Find marauder faction, should be this hardcoded?
        var faction = Faction.All.First(f => f.id == 0);
        
        var leader = CreateInstance<Character>();
        leader.name = "Bandit Leader";
        leader.id = Character.All.OrderByDescending(c => c.id).First().id++;
        leader.faction = faction;
        leader.type = CharacterType.Bandit;
        
        var party = CreateInstance<Party>();
        party.name = "Bandits";
        party.leader = leader;
        party.skin = Random.Range(0, 2);
        party.behavior = faction.behavior;
        
        var count = Random.Range(1, 5);
        for (var i = 0; i < count; i++) {
            party.troops.Add(faction.troops[Random.Range(0, faction.troops.Length)].Clone());
        }

        Party.All.Add(party);
        Character.All.Add(leader);
        
        
        
        var army = Instantiate(Manager.global.armyPrefab, pos, rot).GetComponent<Army>();
        army.data = party;
    }
    
    public void Destroy(bool withLeader = false)
    {
        Party.All.Remove(this);
        ScriptableObject.Destroy(this);
        if (withLeader) {
            Character.All.Remove(leader);
            ScriptableObject.Destroy(leader);
        }
    }

    public static Party Create(PartySave save)
    {
        var obj = CreateInstance<Party>();
        obj.leader = Character.All.First(c => c.id == save.leader);
        obj.name = save.name;
        return obj;
    }
    
    public static Party Copy(Party party)
    {
        var obj = Instantiate(party);
        obj.name = obj.name.Replace("(Clone)", "");
        return obj;
    }

    public void Load(PartySave save = null)
    {
        if (save != null) {
            morale = save.morale;
            speed = save.speed;
            position = save.position;
            rotation = save.rotation;
            troops = save.troops;
            if (save.localSettlement != -1) localSettlement = Settlement.All.First(s => s.id == save.localSettlement);
            if (save.targetSettlement != -1) targetSettlement = Settlement.All.First(s => s.id == save.targetSettlement);
            if (save.enemyParty != -1) enemyParty = Party.All.First(p => p.leader.id == save.enemyParty);
            behavior = save.behavior;
            skin = save.skin;
        } else {
            if (localSettlement) localSettlement = Settlement.All.First(s => s.id == localSettlement.id);
            if (targetSettlement) targetSettlement = Settlement.All.First(s => s.id == targetSettlement.id);
        }
    }
}

[Serializable]
public enum PartyFSM
{
    Holding,
    Traveling,
    Chasing,
    Escaping
}

[Serializable]
public class PartySave
{
    [HideInInspector] public int leader;
    [HideInInspector] public string name;
    [HideInInspector] public float morale;
    [HideInInspector] public float speed;
    [HideInInspector] public Vector3 position;
    [HideInInspector] public Quaternion rotation; 
    [HideInInspector] public List<Troop> troops;
    [HideInInspector] public int localSettlement;
    [HideInInspector] public int targetSettlement;
    [HideInInspector] public int enemyParty;
    [HideInInspector] public ExternalBehavior behavior;
    [HideInInspector] public int skin;

    public PartySave(Party party)
    {
        leader = party.leader.id; // cant be null
        name = party.name;
        morale = party.morale;
        speed = party.speed;
        position = party.position;
        rotation = party.rotation;
        troops = party.troops;
        localSettlement = party.localSettlement ? party.localSettlement.id : -1;
        targetSettlement = party.targetSettlement ? party.targetSettlement.id : -1;
        enemyParty = party.enemyParty ? party.enemyParty.leader.id : -1;
        behavior = party.behavior;
        skin = party.skin;
    }
}