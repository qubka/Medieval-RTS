using System;
using System.Collections.Generic;
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

    public List<Range> ranges;
}

[Serializable]
public struct Range
{
    public float height;
    public float position;
    public int accuracy;
}