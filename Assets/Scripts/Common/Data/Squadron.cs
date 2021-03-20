using System;
using UnityEngine;

[CreateAssetMenu]
[Serializable]
public class Squadron : ScriptableObject
{
    [Header("Unit")]
    public GameObject[] unitPrefabs;
    public GameObject selectorPrefab;
    public float selectorHeight;
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
    public Group groupSounds;
    public Commander commanderSounds;
    public Sprite canvasIcon;
    public Sprite layoutIcon;

    [Space(10)]
    
    [Header("Attack")]
    public int attack;
    public int chargeBonus;
    public Weapon weapon;
    public bool canKnockdown;

    [Header("Defense")]
    public int armour;
    public int defenceSkill;
    public int shield;
    public int hitPoints;
    public int morale;
    public bool canBlock;
    public bool hasShield;
}