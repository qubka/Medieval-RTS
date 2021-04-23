using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Weapons/Melee", order = 0)]
[Serializable]
public class MeleeWeapon : ScriptableObject
{
    [Header("General")]
    public int baseDamage;
    public int armorPiercingDamage;
    public bool armorPiercing;
    public int attackAgainstCavalry;
    public int attackAgainstInfantry;

    [Header("Animation")]
    public float kick;
    public float normal;
    public float distant;
}