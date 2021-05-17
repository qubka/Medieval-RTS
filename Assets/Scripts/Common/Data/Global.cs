using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Global Config", order = 0)]
[Serializable]
public class Global : ScriptableObject
{
    [Header("Hot Keys")]
    public KeyCode addKey = KeyCode.LeftShift;
    public KeyCode inclusiveKey = KeyCode.LeftControl;
    public KeyCode shiftKey = KeyCode.LeftAlt;
    public KeyCode drawKey = KeyCode.LeftAlt;
    public KeyCode stopKey = KeyCode.Escape;

    [Header("Prefabs")] 
    public GameObject armyPrefab;
    public GameObject squadPrefab;
    [Space]
    public GameObject troopLayout;
    public GameObject troopCard;
    public GameObject townIcon;
    [Space]
    public GameObject movementLine;
    public GameObject arrowLine;
    public GameObject directionLine;
    public GameObject drawLine;
    public GameObject moveParticle;
    public GameObject attackParticle;

    [Header("Sounds")]
    public AudioClip moveSound;
    public AudioClip attackSound;
    public AudioClip placeSound;
    public AudioClip selectSound;
    public AudioClip targetSound;
    
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
    
    [Header("Cursors")]
    public Texture2D basicCursor;
    public Texture2D moveCursor;
    public Texture2D invalidCursor;
    public Texture2D meleeCursor;
    public Texture2D rangeCursor;
    public Texture2D dragCursor;
    public Texture2D drawCursor;
    public Texture2D selectCursor;
    public Texture2D placeCursor;
    public Texture2D shiftCursor;
    public Texture2D addCursor;
    public Texture2D inclusiveCursor;
    public Texture2D lookCursor;

    [Header("Other")]
    public GUIStyle rectangleStyle;
}