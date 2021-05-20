﻿using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEditor;
using UnityEngine;
using UnityJSON;

[CreateAssetMenu(menuName = "Medieval/Faction Config", order = 0)]
[Serializable]
public class Faction : SerializableObject
{
    [Header("General")]
    public int id;
    public string label;
    public Color32 color;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Character leader;
    
    [Header("Relationship")]
    [JSONNode(NodeOptions.DontSerialize)]
    public List<Faction> allies;
    [JSONNode(NodeOptions.DontSerialize)]
    public List<Faction> enemies;

    [Header("Initial")]
    [JSONNode(NodeOptions.DontSerialize)] 
    public Troop[] troops;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Model[] models;
    
    #region Serialization
    
    /* For serialization */
    [JSONNode] private int leaderId;
    [JSONNode] private int[] alliedFactions;
    [JSONNode] private int[] enemyFactions;

    public override void OnSerialization()
    {
        leaderId = leader ? leader.id : -1;
        alliedFactions = new int[allies.Count];
        for (var i = 0; i < allies.Count; i++) {
            alliedFactions[i] = allies[i].id;
        }
        enemyFactions = new int[enemies.Count];
        for (var i = 0; i < enemies.Count; i++) {
            enemyFactions[i] = enemies[i].id;
        }
    }

    public override void OnDeserialization()
    {
        var game = SaveLoadManager.Instance.current;
        if (leaderId != -1) {
            leader = game.characters.Find(c => c.id == leaderId);
        }
        allies.Capacity = alliedFactions.Length;
        foreach (var alliedId in alliedFactions) {
            allies.Add(game.factions.Find(f => f.id == alliedId));
        }
        if (alliedFactions.Length > 0) {
            alliedFactions = new int[0];
        }
        enemies.Capacity = enemyFactions.Length;
        foreach (var enemyId in enemyFactions) {
            enemies.Add(game.factions.Find(f => f.id == enemyId));
        }
        if (enemyFactions.Length > 0) {
            enemyFactions = new int[0];
        }
        var faction = Manager.defaultFactions.Find(f => f.id == id);
        troops = faction.troops;
        models = faction.models;
    }
    
    #endregion
}