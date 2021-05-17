using System;
using System.Collections.Generic;
using UnityEngine;
using UnityJSON;
using Object = UnityEngine.Object;

[Serializable]
public class Troop
{
    [Header("General")] 
    public int size;
    public Squadron data;

    [Header("Models")] 
    public GameObject[] primaryPrefabs;
    public GameObject[] secondaryPrefabs;

    // Hide in inspector
    [HideInInspector] public TroopCard card;
    [HideInInspector] public TroopLayout layout;
    public bool isSelect { get; private set; }
    
    public void Destroy()
    {
        Object.DestroyImmediate(card.gameObject);
        Object.DestroyImmediate(layout.gameObject);
    }
    
    public void Select(bool value)
    {
        if (isSelect == value) 
            return;

        card.Select(value);
        isSelect = value;
    }
}