using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Global Config", order = 0)]
[Serializable]
public class Global : ScriptableObject
{
    [Header("Prefabs")] 
    public GameObject armyPrefab;
    public GameObject squadPrefab;
    
    [Header("Layers")]
    public LayerMask ground = -1;
    public LayerMask army = -1;
    public LayerMask unit = -1;
    public LayerMask obstacle = -1;
    public LayerMask building = -1;
    public LayerMask squad = -1;
    public LayerMask water = -1;
    
    [Header("Attributes")] 
    public MoraleAttribute chargedInFlank;
    public MoraleAttribute chargedInRear;
    public MoraleAttribute disordered;
    public MoraleAttribute exhausted;
    public MoraleAttribute flanksProtected;
    public MoraleAttribute flanksThreatened;
    public MoraleAttribute foodVariety;
    public MoraleAttribute generalAround;
    public MoraleAttribute generalWound;
    public MoraleAttribute losingAttack;
    public MoraleAttribute losingBattle;
    public MoraleAttribute lowGround;
    public MoraleAttribute marauding;
    public MoraleAttribute noEnemies;
    public MoraleAttribute noRetreatOption;
    public MoraleAttribute outnumberEnemy;
    public MoraleAttribute outnumbered;
    public MoraleAttribute rain;
    public MoraleAttribute routingEnemies;
    public MoraleAttribute routingFriends;
    public MoraleAttribute starvation;
    public MoraleAttribute totallyExhausted;
    public MoraleAttribute underFire;
    public MoraleAttribute unity;
    public MoraleAttribute uphillPosition;
    public MoraleAttribute veryTired;
    public MoraleAttribute winningBattle;
    public MoraleAttribute withoutAmmo;
}