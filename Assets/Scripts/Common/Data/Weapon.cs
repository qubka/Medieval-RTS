using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
[Serializable]
public class Weapon : ScriptableObject
{
    [Header("General")]
    public bool armorPiercing;

    [Header("Animation")]
    public float kick;
    public float normal;
    public float distant;
}