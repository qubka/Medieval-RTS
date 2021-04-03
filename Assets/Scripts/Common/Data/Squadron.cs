using System;
using GPUInstancer;
using UnityEngine;

[CreateAssetMenu]
[Serializable]
public class Squadron : ScriptableObject
{
    [Header("Unit")]
    public GameObject selectorPrefab;
    public Vector3 selectorPosition;
    public Vector3 attachPosition;
    public UnitSize unitSize;
    public float unitSpeed;
    public float unitAccel;
    public float unitRotation;
    public Animations animations;

    [Header("Squad")]
    public int squadSize;
    public float squadSpeed;
    public float squadAccel;
    public float squadRotation;
    public float attackDistance;
    public float meleeDistance;
    public float chargeDistance;
    public float rangeDistance;
    public Group groupSounds;
    public Commander commanderSounds;
    public Sprite canvasIcon;
    public Sprite layoutIcon;

    [Space(10)]
    
    [Header("Damage")]
    public int attack;
    public int chargeBonus;
    public Weapon melee;
    public Weapon range;
    public bool canKnockdown;

    [Header("Defense")]
    public int armour;
    public int defenceSkill;
    public int shield;
    public int hitPoints;
    public int morale;
    public bool canBlock;
    public bool canCounter;
    public bool hasShield;
    
    public int TotalStats() {
        
        var damage = attack;
        if (melee && melee.armorPiercing) damage *= 2;
        if (range && range.armorPiercing) damage *= 2;
        damage += chargeBonus;
        var defense = armour;
        if (canBlock) defense *= 2;
        if (canCounter) defense *= 2;
        return damage + defense;
    }
}