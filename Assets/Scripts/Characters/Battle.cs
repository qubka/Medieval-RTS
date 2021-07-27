using System;
using UnityEngine;

public class Battle : MonoBehaviour
{
    public Party party1;
    public Party party2;

    private void Start()
    {
        Debug.Log("Created" + party1.leader.name + " " + party2.leader.name);
    }

    public bool Equals(Party party)
    {
        var id = party.leader.id;
        return party1.leader.id == id || party2.leader.id == id;
    }
}