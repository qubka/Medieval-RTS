﻿using System;
using System.Collections.Generic;
 using System.Linq;
 using BehaviorDesigner.Runtime;
using UnityEditor;
using UnityEngine;
using UnityJSON;

[CreateAssetMenu(menuName = "Medieval/Faction Config", order = 0)]
[Serializable]
[InitializeOnLoad]
public class Faction : SerializableObject
{
    [Header("General")]
    public string label;
    public Color color;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Character leader;

    [Header("Relationship")]
    public List<Faction> allies;
    public List<Faction> enemies;
    public float defaultDisposition;
    private Dictionary<Faction, float> cache = new Dictionary<Faction, float>();

    [Header("Initial")]
    [JSONNode(NodeOptions.DontSerialize)] 
    public Troop[] troops;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Model[] models;
    [JSONNode(NodeOptions.DontSerialize)] 
    public ExternalBehaviorTree behavior;
    
    [JSONNode]
    private string[] alliedFactions;
    [JSONNode] 
    private string[] enemyFactions;
    [JSONNode] 
    private string leaderName;

    public float RelationshipWith(Faction other)
    {
        if (cache.ContainsKey(other)) {
            return cache[other];
        }

        var output = defaultDisposition;
        
        foreach (var faction in allies) {
            if (faction == other) {
                output = 1f;
                break;
            }
        }
        
        foreach (var faction in enemies) {
            if (faction == other) {
                output = -1f;
                break;
            }
        }
        
        cache.Add(other, output);
        return output;
    }
    
    public void OnEnable()
    {
        hash = label.GetHashCode();
    }

    public override void OnSerialization()
    {
        leaderName = leader.surname;
        alliedFactions = new string[allies.Count];
        for (var i = 0; i < allies.Count; i++) {
            alliedFactions[i] = allies[i].label;
        }
        enemyFactions = new string[enemies.Count];
        for (var i = 0; i < enemies.Count; i++) {
            enemyFactions[i] = enemies[i].label;
        }
    }

    public override void OnDeserialization()
    {
        var faction = Manager.defaultFactions.Find(f => f & this);
        troops = faction.troops;
        models = faction.models;
        behavior = faction.behavior;
    }
}