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
    public Gradient nightBlend;
    
    [Header("SkyBox")]
    public Material sunny;
    public Material night;
    public Material rainy;
}