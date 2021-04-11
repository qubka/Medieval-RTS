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

    [Space(10)]
    
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
    
    public int TotalStats()
    {
        var damage = meleeAttack;
        if (meleeWeapon) damage += meleeWeapon.baseDamage + meleeWeapon.armorPiercingDamage;
        if (rangeWeapon) damage += rangeWeapon.missileDamage + rangeWeapon.missileArmorPiercingDamage;
        damage += chargeBonus;
        var defense = armor + shield + defenceSkill + manHealth + bonusHitPoints + mountHealth;
        return damage + defense;
    }
}