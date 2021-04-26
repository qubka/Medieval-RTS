﻿using System;
using System.Collections.Generic;
 using BehaviorDesigner.Runtime;
 using UnityEditor;
 using UnityEngine;
 
[CreateAssetMenu(menuName = "Medieval/Faction Config", order = 0)]
[Serializable]
[InitializeOnLoad]
public class Faction : ScriptableObject
{
    [Header("General")]
    public string label;
    public Color color;
    public Character leader;
    public List<Character> characters;
    public List<Party> parties;

    [Header("Relationship")]
    public List<Location> locations;
    public List<Faction> allies;
    public List<Faction> enemies;
    public float defaultDisposition;
    [NonSerialized]
    private Dictionary<Faction, float> cache = new Dictionary<Faction, float>();

    [Header("Other")]
    public Troop[] troops;
    public Model[] models;
    public ExternalBehaviorTree behavior;

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
    
    [NonSerialized] public int id;

    private void OnEnable()
    {
        id = name.GetHashCode(); 
    }
    
    public override int GetHashCode()
    {
        return id;
    }
}