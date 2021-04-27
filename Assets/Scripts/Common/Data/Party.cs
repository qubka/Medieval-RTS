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
    public List<Troop> troops;
    //public Party followingParty;
    //public Location followingLocation;
    //public PartyFSM state;
    public int skin;
    
    [JSONNode] 
    private string leaderName;
    
    public void OnEnable()
    {
        hash = leader.surname.GetHashCode();
    }

    public override void OnSerialization()
    {
        leaderName = leader.surname;
    }

    public override void OnDeserialization()
    {
        var game = SaveLoadManager.Instance.current;
        leader = game.characters.Find(c => c.surname.Equals(leaderName));
    }
}

[Serializable]
public enum PartyFSM
{
    Holding,
    Travel
}