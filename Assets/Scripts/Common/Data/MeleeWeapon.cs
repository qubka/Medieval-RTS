using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu]
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