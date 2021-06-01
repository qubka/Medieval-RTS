﻿using System;
using System.Collections.Generic;
 using System.Linq;
 using BehaviorDesigner.Runtime;
 using Unity.Mathematics;
 using UnityEditor;
using UnityEngine;
using UnityJSON;

[CreateAssetMenu(menuName = "Medieval/Templates/Faction", order = 0)]
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

    public static List<Faction> All => Game.Factions;
    public int TotalStrength => Party.All.Where(p => p.leader.faction == this).Select(p => p.TroopStrength).Sum();

    public List<Settlement> Settlements => Settlement.All.Where(s => s.ruler.faction == this).ToList();
    public List<Character> Characters => Character.All.Where(c => c.faction == this).ToList();
    public List<House> Houses => House.All.Where(h => h.leader.faction == this).ToList();

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
        if (leaderId != -1) {
            leader = Game.Characters.Find(c => c.id == leaderId);
        }
        allies.Capacity = alliedFactions.Length;
        foreach (var alliedId in alliedFactions) {
            allies.Add(Game.Factions.Find(f => f.id == alliedId));
        }
        if (alliedFactions.Length > 0) {
            alliedFactions = new int[0];
        }
        enemies.Capacity = enemyFactions.Length;
        foreach (var enemyId in enemyFactions) {
            enemies.Add(Game.Factions.Find(f => f.id == enemyId));
        }
        if (enemyFactions.Length > 0) {
            enemyFactions = new int[0];
        }
        var faction = Manager.defaultFactions.Find(f => f.id == id);
        troops = faction.troops;
        models = faction.models;
    }
    
    #endregion

    public bool IsStrong()
    {
        var medianStrength = GetMedianStrength();
        return TotalStrength > medianStrength;
    }
    
    private float GetMedianStrength()
    {
        float medianStrength;
        
        var factionStrengths = All.Select(curKingdom => curKingdom.TotalStrength).OrderBy(a => a).ToArray();
        var halfIndex = factionStrengths.Count() / 2;

        if ((factionStrengths.Length % 2) == 0) {
            medianStrength = (factionStrengths.ElementAt(halfIndex) + factionStrengths.ElementAt(halfIndex - 1)) / 2f;
        } else {
            medianStrength = factionStrengths.ElementAt(halfIndex);
        }
        return medianStrength;
    }
    
    public float GetAllianceStrength()
    {
        return allies.Select(curKingdom => curKingdom.TotalStrength).Sum() + TotalStrength;
    }
    
    public float GetExpansionism()
    {
        return 0;//ExpansionismManager.Instance.GetExpansionism(this);
    }

    public float GetExpansionismDiplomaticPenalty()
    {
        return math.min(-(GetExpansionism() - 50f), 0f);
    }

    public float GetMinimumExpansionism()
    {
        return 0f;//ExpansionismManager.Instance!.GetMinimumExpansionism(faction);
    }
}