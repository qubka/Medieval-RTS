using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Medieval/Resource Config", order = 0)]
[Serializable]
public class Resource : ScriptableObject
{
    public int id;
    public string label;
    public Sprite icon;
}