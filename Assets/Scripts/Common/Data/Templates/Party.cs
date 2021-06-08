using System;
using System.Collections.Generic;
using System.Linq;
using Den.Tools;
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
    public List<Troop> troops;
    public Settlement localSettlement;
    public Settlement targetSettlement; // for AI
    public int skin;

    public static List<Party> All => Game.Parties;
    public int TroopStrength => troops.Sum(t => t.data.TotalStats);
    public int TroopCount => troops.Sum(t => t.size);
    public int TroopWage => -Convert.ToInt32(troops.Sum(t => t.data.recruitCost * ((float) t.size / t.data.maxCount)));

    public static void CreatePeasant(Settlement settlement)
    {
        var leader = CreateInstance<Character>();
        leader.name = "Peasant";
        leader.id = Character.All.OrderByDescending(c => c.id).First().id++;
        leader.faction = settlement.ruler.faction;
        leader.type = CharacterType.Peasant;
        leader.home = settlement;
        
        var party = CreateInstance<Party>();
        party.name = "Peasants";
        party.leader = leader;
        party.skin = Random.Range(0, 2);

        var count = math.min(math.max(1, settlement.prosperity / 10), 3);
        if (party.troops == null) party.troops = new List<Troop>(count);
        for (var i = 0; i < count; i++) {
            var troops = Manager.global.troops;
            party.troops.Add(troops[Random.Range(0, troops.Length)].Clone());
        }
        
        Party.All.Add(party);
        Character.All.Add(leader);
        
        var army = Instantiate(Manager.global.armyPrefab, settlement.position, Quaternion.identity).GetComponent<Army>();
        army.data = party;
        army.data.targetSettlement = settlement.neighbours[0];
        army.behavior = Manager.global.behavior;
    }
    
    public void DestroyParty(bool withLeader = false)
    {
        Party.All.Remove(this);
        Destroy(this);
        if (withLeader) {
            Character.All.Remove(leader);
            Destroy(leader);
        }
    }

    public static Party Create(PartySave save)
    {
        var obj = CreateInstance<Party>();
        obj.leader = Character.All.First(c => c.id == save.leader);
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
            skin = save.skin;
        } else {
            if (localSettlement) localSettlement = Settlement.All.First(s => s.id == localSettlement.id);
            if (targetSettlement) targetSettlement = Settlement.All.First(s => s.id == targetSettlement.id);
        }
    }

    public Party Clone()
    {
        var obj = Instantiate(this);
        obj.name = obj.name.Replace("(Clone)", "");
        return obj;
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
    [HideInInspector] public float morale;
    [HideInInspector] public float speed;
    [HideInInspector] public Vector3 position;
    [HideInInspector] public Quaternion rotation; 
    [HideInInspector] public List<Troop> troops;
    [HideInInspector] public int localSettlement;
    [HideInInspector] public int targetSettlement;
    [HideInInspector] public int skin;

    public PartySave(Party party)
    {
        leader = party.leader.id; // cant be null
        morale = party.morale;
        speed = party.speed;
        position = party.position;
        rotation = party.rotation;
        troops = party.troops;
        localSettlement = party.localSettlement ? party.localSettlement.id : -1;
        targetSettlement = party.targetSettlement ? party.targetSettlement.id : -1;
        skin = party.skin;
    }
}