using System;
using System.Collections.Generic;
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
    //public Party followingParty;
    //public Location followingLocation;
    public PartyFSM state;
    public int skin;
    
    [JSONNode] private int leaderId;
    [JSONNode] private Pack<int, int>[] troopsData;

    public override void OnSerialization()
    {
        leaderId = leader ? leader.id : -1;
        troopsData = new Pack<int, int>[troops.Count];
        for (var i = 0; i < troops.Count; i++) {
            var troop = troops[i];
            troopsData[i] = new Pack<int, int>(Array.IndexOf(leader.faction.troops, troop), troop.size);
        }
    }

    public override void OnDeserialization()
    {
        var game = SaveLoadManager.Instance.current;
        if (leaderId != -1) {
            leader = game.characters.Find(c => c.id == leaderId);
        }
        troops.Capacity = troopsData.Length;
        foreach (var pack in troopsData) {
            var troop = leader.faction.troops[pack.item1];
            troop.size = pack.item2;
            troops.Add(troop);
        }
        troopsData = null;
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