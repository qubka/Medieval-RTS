using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Sounds/Community", order = 0)]
[Serializable]
public class Community : ScriptableObject
{
    public AudioClip daySound;
    public AudioClip nightSound;
}