using System;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Squadron Config", order = 0)]
[Serializable]
[InitializeOnLoad]
public class Squadron : ScriptableObject
{
    [Header("Army")]
    public int recruitCost;
    public int maxCount;

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
    public float squadWalkSpeed;
    public float squadRunSpeed;
    public float squadAccel;
    public float squadRotation;
    public float attackDistance;
    public float meleeDistance;
    public float chargeDistance;
    public float rangeDistance;
    public Group groupSounds;
    public Commander commanderSounds;
    public Sprite classIcon;
    public Sprite bigIcon;
    public Sprite smallIcon;

    [Space(10f)]
    
    [Header("Melee")]
    public int meleeAttack;
    public MeleeWeapon meleeWeapon;
    public int chargeBonus;

    [Header("Range")]
    public RangeWeapon rangeWeapon;
    public int accuracy;
    public int ammunition;
    public float fireRate;
    
    [Header("Defense")]
    public int armor;
    public int shield;
    public int defenceSkill;
    public int manHealth;
    public int bonusHitPoints;
    public int mountHealth;
    public int morale;
    public bool chargeProtection;

    [Header("Battle")] 
    public UnitType type;
    public UnitType advantage;

    public Stats Stats { get; private set; }
    public int TotalStats { get; private set; }
    
    private void OnEnable()
    {
        var attack = meleeAttack;
        if (meleeWeapon) attack += meleeWeapon.baseDamage + meleeWeapon.armorPiercingDamage;
        if (rangeWeapon) attack += rangeWeapon.missileDamage + rangeWeapon.missileArmorPiercingDamage;
        attack += chargeBonus;
        var defence = defenceSkill + armor + shield;
        var health = manHealth + mountHealth;
        Stats = new Stats(attack, defence, Mathf.RoundToInt(squadRunSpeed * 10f), morale, health);
        TotalStats = attack + defence + health;
    }
}

[Serializable]
public enum UnitType
{
    Archers,
    Pikemen,
    Spearmen,
    LightInfantry,
    HeavyInfantry,
    Cavalry
}