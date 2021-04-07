using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu]
public class RangeWeapon : ScriptableObject
{
    [Header("General")]
    public int missileDamage;
    public int missileArmorPiercingDamage;

    [Header("Animation")]
    public float close;
    public float distant;
}