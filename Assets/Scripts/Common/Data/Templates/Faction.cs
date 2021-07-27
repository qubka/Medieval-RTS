﻿using System;
using System.Collections.Generic;
using System.Linq;
 using BehaviorDesigner.Runtime;
 using Den.Tools;
 using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Templates/Faction", order = 0)]
[Serializable]
public class Faction : ScriptableObject
{
    [Header("General")]
    public int id;
    public string label;
    public Color32 color;
    public Character leader;
    
    [Header("Relationship")]
    public List<Faction> allies = new List<Faction>();
    public List<Faction> enemies = new List<Faction>();

    [Header("Initial")]
    public Troop[] troops;
    public Model[] models;
    public ExternalBehavior behavior;

    public static List<Faction> All => Game.Factions;
    public int TotalStrength => Party.All.Where(p => p.leader.faction == this).Select(p => p.TroopStrength).Sum();

    public List<Settlement> Settlements => Settlement.All.Where(s => s.ruler.faction == this).ToList();
    public List<Character> Characters => Character.All.Where(c => c.faction == this).ToList();
    public List<House> Houses => House.All.Where(h => h.leader.faction == this).ToList();

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

    public static Faction Create(FactionSave save)
    {
        var obj = CreateInstance<Faction>();
        obj.id = save.id;
        obj.name = save.name;
        return obj;
    }
    
    public static Faction Copy(Faction faction)
    {
        var obj = Instantiate(faction);
        obj.name = obj.name.Replace("(Clone)", "");
        return obj;
    }

    public void Load(FactionSave save = null)
    {
        if (save != null) {
            label = save.label;
            color = save.color;
            if (save.leader != -1) leader = Character.All.First(c => c.id == save.leader);
            allies = Faction.All.Where(f => save.allies.Contains(f.id)).ToList();
            enemies = Faction.All.Where(f => save.enemies.Contains(f.id)).ToList();
            troops = save.troops;
            models = save.models;
            behavior = save.behavior;
        } else {
            if (leader) leader = Character.All.First(c => c.id == leader.id);
            for (var i = 0; i < allies.Count; i++) {
                allies[i] = Faction.All.First(f => f.id == allies[i].id);
            }
            for (var i = 0; i < enemies.Count; i++) {
                enemies[i] = Faction.All.First(f => f.id == enemies[i].id);
            }
        }
    }
}

[Serializable]
public class FactionSave
{
    [HideInInspector] public int id;
    [HideInInspector] public string name;
    [HideInInspector] public string label;
    [HideInInspector] public Color32 color;
    [HideInInspector] public int leader;
    [HideInInspector] public int[] allies;
    [HideInInspector] public int[] enemies;
    [HideInInspector] public Troop[] troops;
    [HideInInspector] public Model[] models;
    [HideInInspector] public ExternalBehavior behavior;
    
    public FactionSave(Faction faction)
    {
        id = faction.id;
        name = faction.name;
        label = faction.label;
        color = faction.color;
        leader = faction.leader ? faction.leader.id : -1;
        allies = faction.allies.Select(f => f.id).ToArray();
        enemies = faction.enemies.Select(f => f.id).ToArray();;
        troops = faction.troops;
        models = faction.models;
        behavior = faction.behavior;
    }
}