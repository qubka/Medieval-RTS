using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Banner Config", order = 0)]
[Serializable]
public class Banner : ScriptableObject
{
    public GameObject clearTown;
    public GameObject clearArmy;
    public GameObject clearWall;
    [Space]
    public GameObject tornTown;
    public GameObject tornArmy;
    public GameObject tornWall;
}