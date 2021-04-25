﻿using System;
using System.Collections.Generic;
using UnityEngine;
 
[CreateAssetMenu(menuName = "Medieval/Faction Config", order = 0)]
[Serializable]
public class Faction : ScriptableObject
{
    [Header("General")]
    public string label;
    public Color color;
    public Character leader;
    public List<Character> characters;

    [Header("Relationship")]
    public List<Location> locations;
    public List<Faction> allies;
    public List<Faction> enemies;
    public float defaultDisposition;
    public Dictionary<Faction, float> cache = new Dictionary<Faction, float>();
    
    [Header("Other")]
    public Troop[] troops;
    public Model[] models;

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
    
    

    //public List<string> relationships;
    //
    //public List<Squadron> factionTroops;

    //[ReadOnly] public List<Character> charaters;
}