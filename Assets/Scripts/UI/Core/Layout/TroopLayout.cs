using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class TroopLayout : Layout<Troop>
{
    private void Start()
    {
        manager = ArmyManager.Instance;
    }
}