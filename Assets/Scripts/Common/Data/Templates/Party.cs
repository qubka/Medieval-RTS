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
    [ReadOnly, NonSerialized] public Army army;
    public Character leader;
    public float morale;
    public Vector3 position;
    public Quaternion rotation; 
    public List<Troop> troops = new List<Troop>();
    public Settlement localSettlement;
    public Settlement targetSettlement; // for AI
    public ExternalBehavior behavior;
    public int skin;
    public bool inBattle;

    public static List<Party> All => Game.Parties;
    public int TroopStrength => troops.Sum(t => t.size * t.data.TotalStats);
    public int TroopSize => troops.Sum(t => t.size);
    public int TroopCount => troops.Count;
    public int TroopWage => -Convert.ToInt32(troops.Sum(t => t.data.recruitCost * ((float) t.size / t.data.maxCount)));
    public float TroopLength => troops.Sum(t => t.size / t.data.unitSize.height * t.data.unitSize.width);
    public Troop RandomTroop => TroopCount > 0 ? troops[Random.Range(0, TroopCount)] : null;

    public static Army CreatePeasant(Settlement settlement)
    {
        var leader = CreateInstance<Character>();
        leader.name = "Peasant Elder";
        leader.surname = leader.name;
        leader.id = Character.All.OrderByDescending(c => c.id).First().id + 1;
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

        var town = settlement.town;
        return Army.Create(party, town.doorPosition, town.doorRotation);
    }

    public static Army CreateBandit(Vector3 position)
    {
        // Find marauder faction, should be this hardcoded?
        var faction = Faction.All.First(f => f.id == 0);
        
        var leader = CreateInstance<Character>();
        leader.name = "Bandit Leader";
        leader.surname = leader.name;
        leader.id = Character.All.OrderByDescending(c => c.id).First().id + 1;
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

        var randomPosition = Vector.GetRandomNavMeshPositionNearLocation(position, 20f);
        var randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        return Army.Create(party, randomPosition, randomRotation);
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
            if (save.leader != -1) leader = Character.All.First(c => c.id == save.leader);
            morale = save.morale;
            position = save.position;
            rotation = save.rotation;
            troops = save.troops;
            if (save.localSettlement != -1) localSettlement = Settlement.All.First(s => s.id == save.localSettlement);
            if (save.targetSettlement != -1) targetSettlement = Settlement.All.First(s => s.id == save.targetSettlement);
            behavior = save.behavior;
            skin = save.skin;
            inBattle = save.inBattle;
        } else {
            if (leader) leader = Character.All.First(c => c.id == leader.id);
            if (localSettlement) localSettlement = Settlement.All.First(s => s.id == localSettlement.id);
            if (targetSettlement) targetSettlement = Settlement.All.First(s => s.id == targetSettlement.id);
        }
    }
    
    // checks whether a specific type is present in the current army
    public bool IsTroopExist(UnitType type)
    {
        return troops.Any(t => t.data.type == type);
    }

    public Troop GetRandomTroop(UnitType type)
    {
        if (!IsTroopExist(type)) {
            Debug.Log("There are no troops of that type!");
            return RandomTroop;
        }

        var troop = troops.Where(t => t.data.type == type).ToArray();
        return troop.Length > 0 ? troop[Random.Range(0, troop.Length)] : null;
    }

    public void RemoveTroop(Troop troop)
    {
        troops.Remove(troop);
    }

    public void Validate()
    {
        for (var i = troops.Count - 1; i > -1; i--) {
            var troop = troops[i];
            if (troop.size <= 0) {
                RemoveTroop(troop);
            }
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
    //[HideInInspector] public float speed;
    [HideInInspector] public Vector3 position;
    [HideInInspector] public Quaternion rotation; 
    [HideInInspector] public List<Troop> troops;
    [HideInInspector] public int localSettlement;
    [HideInInspector] public int targetSettlement;
    [HideInInspector] public ExternalBehavior behavior;
    [HideInInspector] public int skin;
    [HideInInspector] public bool inBattle;

    public PartySave(Party party)
    {
        leader = party.leader.id; // cant be null
        name = party.name;
        morale = party.morale;
        //speed = party.speed;
        position = party.position;
        rotation = party.rotation;
        troops = party.troops;
        localSettlement = party.localSettlement ? party.localSettlement.id : -1;
        targetSettlement = party.targetSettlement ? party.targetSettlement.id : -1;
        behavior = party.behavior;
        skin = party.skin;
        inBattle = party.inBattle;
    }
}