using System;
using GPUInstancer;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Squadron Config", order = 0)]
[Serializable]
[InitializeOnLoad]
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
    //public int squadSize;
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
    public Sprite canvasIcon;
    public Sprite layoutIcon;

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
    
    [NonSerialized] public Stats stats;
    [NonSerialized] public int totalStats;

    private void OnEnable()
    {
        var attack = meleeAttack;
        if (meleeWeapon) attack += meleeWeapon.baseDamage + meleeWeapon.armorPiercingDamage;
        if (rangeWeapon) attack += rangeWeapon.missileDamage + rangeWeapon.missileArmorPiercingDamage;
        attack += chargeBonus;
        var defence = defenceSkill + armor + shield;
        var health = manHealth + mountHealth;
        stats = new Stats(attack, defence, Mathf.RoundToInt(squadRunSpeed * 10f), morale, health);
        totalStats = attack + defence + health;
    }
}