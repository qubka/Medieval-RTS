using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityJSON;

[CreateAssetMenu(menuName = "Medieval/Party Config", order = 0)]
[Serializable]
[InitializeOnLoad]
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
    public Town localTown;
    //public PartyFSM state;
    public int skin;
    
    #region Serialization
    
    [JSONNode] private int leaderId;
    [JSONNode] private int settlementId;
    [JSONNode] private Pack<int, int>[] troopsData;
    [JSONNode] private Pack<UI, int> followingData;

    public override void OnSerialization()
    {
        leaderId = leader ? leader.id : -1;
        settlementId = localTown ? localTown.GetID() : -1;
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
        if (settlementId != -1) {
            localTown = TownTable.Instance.Values.First(t => t.GetID() == settlementId);
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
            followingObject = ObjectList.Instance.list.Find(o => o.GetUI() == followingData.item1 && o.GetID() == followingData.item2);
            followingData = null;
        }
    }
    
    #endregion
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