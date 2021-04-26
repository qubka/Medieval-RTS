﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Weapons/Range", order = 0)]
[Serializable]
[InitializeOnLoad]
public class RangeWeapon : ScriptableObject
{
    [Header("General")] 
    public string projectile;
    public int missileDamage;
    public int missileArmorPiercingDamage;

    [Header("Animation")]
    public float close;
    public float distant;
    public Range[] ranges;
    
    [NonSerialized] public int id;

    private void OnEnable()
    {
        id = projectile.GetHashCode(); 
    }
}

[Serializable]
public struct Range
{
    public float height;
    public float position;
    public int accuracy;
}