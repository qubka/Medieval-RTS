using System;
using System.Collections.Generic;
using UnityEngine;
using UnityJSON;
using Object = UnityEngine.Object;

[Serializable]
public class Troop : ISelectable
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
    private bool isSelect;
    
    public void Destroy()
    {
        Object.DestroyImmediate(card.gameObject);
        Object.DestroyImmediate(layout.gameObject);
    }

    public bool IsSelect()
    {
        return isSelect;
    }

    public void Select(bool value)
    {
        if (isSelect == value) 
            return;

        card.Select(value);
        isSelect = value;
    }

    public Troop Clone()
    {
        return (Troop) MemberwiseClone();
    }
}