using System;
using System.Collections.Generic;
using UnityEngine;
using UnityJSON;

[Serializable]
public class Troop
{
    [Header("General")] 
    public int size;
    public Squadron data;

    [Header("Models")] 
    public GameObject[] primaryPrefabs;
    public GameObject[] secondaryPrefabs;
}