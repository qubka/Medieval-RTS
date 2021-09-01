using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

[Serializable]
public class Troop : ISelectable
{
    [Header("General")] 
    public int size;
    public Squadron data;

    [Header("Models")] 
    public GameObject[] primaryPrefabs;
    public GameObject[] secondaryPrefabs;

    //[Header("Other")]
    [NonSerialized] public TroopCard card;
    [NonSerialized] public TroopLayout layout;
    private bool isSelect;
    
    public bool IsStrongAgainst(Troop troop) => data.advantage == troop.data.type;
    
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