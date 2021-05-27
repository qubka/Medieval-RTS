using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityJSON;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Medieval/Party Config", order = 0)]
[Serializable]
public class Party : SerializableObject
{
    [JSONNode(NodeOptions.DontSerialize)] 
    public Character leader;
    public float morale;
    public float speed;
    public Vector3 position;
    public Quaternion rotation;
    [JSONNode(NodeOptions.DontSerialize)] 
    public List<Troop> troops;
    [JSONNode(NodeOptions.DontSerialize)] 
    public IGameObject followingObject;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Settlement localSettlement;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Settlement targetSettlement; // for AI
    //public PartyFSM state;
    public int skin;

    public int troopCount => troops.Sum(t => t.size);
    public int troopWage => -Convert.ToInt32(troops.Sum(t => t.data.recruitCost * ((float) t.size / t.data.maxCount)));
    public static Party Dummy()
    {
        var party = CreateInstance<Party>();
        party.leader = CreateInstance<Character>();
        return party;
    }

    #region Serialization
    
    [JSONNode] private int leaderId;
    [JSONNode] private int localSettlementId;
    [JSONNode] private int targetSettlementId;
    [JSONNode] private Pack<int, int>[] troopsData;
    [JSONNode] private Pack<UI, int> followingData;

    public override void OnSerialization()
    {
        leaderId = leader ? leader.id : -1;
        localSettlementId = localSettlement ? localSettlement.id : -1;
        targetSettlementId = targetSettlement ? targetSettlement.id : -1;
        troopsData = new Pack<int, int>[troops.Count];
        for (var i = 0; i < troops.Count; i++) {
            var troop = troops[i];
            troopsData[i] = new Pack<int, int>(Array.IndexOf(leader.faction.troops, troop), troop.size);
        }
        if (followingObject != null) {
            followingData = new Pack<UI, int>(followingObject.GetUI(), followingObject.GetID());
        }
    }

    public override void OnDeserialization()
    {
        var game = SaveLoadManager.Instance.current;
        if (leaderId != -1) {
            leader = game.characters.Find(c => c.id == leaderId);
        }
        if (localSettlementId != -1) {
            localSettlement = game.settlements.Find(s => s.id == localSettlementId);
        }
        if (targetSettlementId != -1) {
            targetSettlement = game.settlements.Find(s => s.id == targetSettlementId);
        }
        troops.Capacity = troopsData.Length;
        foreach (var pack in troopsData) {
            var troop = leader.faction.troops[pack.item1];
            troop.size = pack.item2;
            troops.Add(troop);
        }
        if (troopsData.Length > 0) {
            troopsData = new Pack<int, int>[0];
        }
        if (followingData != null) {
            followingObject = ObjectTable.Instance.Values.First(o => o.GetUI() == followingData.item1 && o.GetID() == followingData.item2);
            followingData = null;
        }
    }
    
    #endregion
    
    public static void CreatePeasant(Settlement settlement)
    {
        var game = Game.Instance;
        
        var leader = CreateInstance<Character>();
        leader.name = "Peasant";
        leader.id = game.characters.OrderByDescending(c => c.id).First().id++;
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
        
        game.parties.Add(party);
        game.characters.Add(leader);
        
        var army = Instantiate(Manager.global.armyPrefab, settlement.position, Quaternion.identity).GetComponent<Army>();
        army.data = party;
        army.data.targetSettlement = settlement.neighbours[0];
        army.behavior = Manager.global.behavior;
    }
    
    public void DestroyParty(bool withLeader = false)
    {
        var game = Game.Instance;
        game.parties.Remove(this);
        Destroy(this);
        if (withLeader) {
            game.characters.Remove(leader);
            Destroy(leader);
        }
    }
}

[Serializable]
[JSONEnum(format = JSONEnumMemberFormating.Lowercased)]
public enum PartyFSM
{
    Holding,
    Traveling,
    Chasing,
    Escaping
}