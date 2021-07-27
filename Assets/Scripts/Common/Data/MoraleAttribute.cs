using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Morale Attribute Config", order = 0)]
[Serializable]
public class MoraleAttribute : ScriptableObject
{
    public int bonus;
}
