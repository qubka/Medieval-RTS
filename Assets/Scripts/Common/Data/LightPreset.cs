using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Light Preset Config", order = 0)]
[Serializable]
public class LightPreset : ScriptableObject
{
    [Header("Lights")]
    public Gradient ambientColor;
    public Gradient directionColor;
    public Gradient fogColor;
}