using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Party Config", order = 0)]
[Serializable]
public class Party : ScriptableObject
{
    public Character leader;
    public float morale;
    public float speed;
    //public Vector3 position;
    //public Vector3 rotation;
    public List<Troop> troops;
    public Party followingParty;
    public Location followingLocation;
    public ExternalBehaviorTree behavior;
    //public PartyFSM state;
    public int skin;
}

[Serializable]
public enum PartyFSM
{
    Holding,
    Travel
}