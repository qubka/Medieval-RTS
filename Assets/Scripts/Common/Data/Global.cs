using System;
using BehaviorDesigner.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Global Config", order = 0)]
[Serializable]
public class Global : ScriptableObject
{
    //TODO: REWORK KEYS
    [Header("Hot Keys")]
    public KeyCode addKey = KeyCode.LeftShift;
    public KeyCode inclusiveKey = KeyCode.LeftControl;
    public KeyCode shiftKey = KeyCode.LeftAlt;
    public KeyCode drawKey = KeyCode.LeftAlt;
    public KeyCode stopKey = KeyCode.Escape;

    #region Resources

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
    public GameObject deployParticle;

    [Header("Sounds")]
    public AudioClip moveSound;
    public AudioClip attackSound;
    public AudioClip placeSound;
    public AudioClip selectSound;
    public AudioClip targetSound;

    #endregion
    
    #region Layers

    [Header("Layers")]
    public LayerMask ground = -1;
    public LayerMask army = -1;
    public LayerMask unit = -1;
    public LayerMask obstacle = -1;
    public LayerMask building = -1;
    public LayerMask squad = -1;
    public LayerMask water = -1;

    #endregion
    
    #region MoraleAttributes

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

    #endregion
    
    #region Cursors

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

    #endregion

    #region Armies

    [Header("Peasants")] 
    public Troop[] troops;
    public Model[] models;
    public ExternalBehavior behavior;

    #endregion
    
    #region Diplomacy

    [Header("Kingdom Diplomacy")]
    public bool enableFiefFirstRight = true;
    public int minimumWarDurationInDays = 10;
    public int declareWarCooldownInDays = 100;
    public bool enableAlliances = true;
    public int minimumAllianceDuration = 10;
    public int nonAggressionPactDuration = 100;
    public int nonAggressionPactTendency = 0;
    public int allianceTendency = 0;
    public bool playerDiplomacyControl = false;

    [Header("Messengers")]
    public int sendMessengerGoldCost = 100;
    public int messengerTravelTime = 3;

    [Header("War Exhaustion")]
    public bool enableWarExhaustion = true;
    public float maxWarExhaustion = 100f;
    public float warExhaustionPerDay = 1.0f;
    public float warExhaustionPerCasualty = 0.01f;
    public float warExhaustionPerSiege = 10f;
    public float warExhaustionPerRaid = 3f;
    public float warExhaustionDecayPerDay = 2.0f;
    public bool enableWarExhaustionDebugMessages = false;

    [Header("Relations")]
    public float grantFiefPositiveRelationMultiplier = 1.0f;
    public int grantFiefRelationPenalty = -2;

    [Header("Costs")]
    public float scalingWarReparationsGoldCostMultiplier = 100.0f;
    public int declareWarInfluenceCost = 100;
    public int makePeaceInfluenceCost = 100;
    public bool enableInfluenceCostsForDiplomacyActions = true;
    public bool scalingInfluenceCosts = true;
    public float scalingInfluenceCostMultiplier = 5.0f;
    
    [Header("Influence")]
    public bool enableInfluenceBalancing = true;
    public int maximumInfluenceLoss = 20;
    public bool enableInfluenceDecay = true;
    public int influenceDecayThreshold = 1000;
    public float influenceDecayPercentage = 2f;
    public bool enableCorruption = true;

    [Header("Expansionism")]
    public int minimumExpansionismPerFief = 3;
    public int expanisonismPerSiege = 20;
    public int expansionismDecayPerDay = 1;

    [Header("Misc")]
    public bool enableStorylineProtection = true;
    public bool enableUsurpThroneFeature = false;

    public bool enableCoalitions = false;
    public float coalitionChancePercentage = 5.0f;
    public int criticalExpansionism = 100;

    [Header("Civil Wars")]
    public float dailyChanceToStartRebelFaction = 0.05f;
    public float dailyChanceToJoinRebelFaction = 0.1f;
    public float dailyChanceToStartCivilWar = 0.1f;
    public int minimumTimeSinceLastCivilWarInDays = 240;
    public int maximumFactionDurationInDays = 120;
    public int factionCreationInfluenceCost = 100;
    public int factionTendency = 0;
    
    #endregion
    
    [Header("Other")]
    public GUIStyle rectangleStyle;
    public int maxTroops = 10;
}