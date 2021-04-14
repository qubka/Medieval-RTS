﻿using GPUInstancer.CrowdAnimations;
using UnityEngine;
 
[RequireComponent(typeof(Manager))]
public class Manager : MonoBehaviour
{
	[Header("Refs")]
	public GPUICrowdManager crowdManager;
	public RectTransform squadFrames;
	public RectTransform unitLayout;
	public RectTransform unitCard;
	public Camera main;
	public Camera minimap;
	public CombatSliderRatio combatSliderRatio;

	[Header("Layers")]
	public LayerMask ground = -1;
	public LayerMask unit = -1;
	public LayerMask obstacle = -1;
	public LayerMask building = -1;
	public LayerMask squad = -1;
	public LayerMask manager = -1;
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

	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	
	public static int ChargedInFlank;
	public static int ChargedInRear;
	public static int Disordered;
	public static int Exhausted;
	public static int FlanksProtected;
	public static int FlanksThreatened;
	public static int FoodVariety;
	public static int GeneralAround;
	public static int GeneralWound;
	public static int LosingAttack;
	public static int LosingBattle;
	public static int LowGround;
	public static int Marauding;
	public static int NoEnemies;
	public static int NoRetreatOption;
	public static int OutnumberEnemy;
	public static int Outnumbered;
	public static int Rain;
	public static int RoutingEnemies;
	public static int RoutingFriends;
	public static int Starvation;
	public static int TotallyExhausted;
	public static int UnderFire;
	public static int Unity;
	public static int UphillPosition;
	public static int VeryTired;
	public static int WinningBattle;
	public static int WithoutAmmo;
	
	public static int Ground;
	public static int Unit;
	public static int Obstacle;
	//public static int Building;
	public static int Squad;
	//public static int Manager;
	public static int Water;
	
	public static Terrain terrain;
	public static TerrainBorder border;
	public static Camera mainCamera;
	public static Camera minimapCamera;
	public static Transform camTransform;
	public static CamController camController;
	public static RectTransform squadCanvas;
	public static RectTransform layoutCanvas;
	public static RectTransform cardCanvas;
	public static ObjectPool objectPool;
	public static UnitTable unitTable;
	public static UnitManager unitManager;
	public static SoundManager soundManager;
	public static GPUICrowdManager modelManager;
	public static CombatSliderRatio combatSlider;
	public static AudioSource[] cameraSources;
	
	public static readonly int Selector = "SelectorPoint".GetHashCode();
	public static readonly int Pointer = "PointerMove".GetHashCode();
	public static readonly int Way = "Way".GetHashCode();
	
	public static readonly int GrayscaleAmount = Shader.PropertyToID("_GrayscaleAmount");
	public static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

	private void Awake()
	{
		terrain = Terrain.activeTerrain;
		border = terrain.GetComponent<TerrainBorder>();
		modelManager = crowdManager;
		mainCamera = main;
		minimapCamera = minimap;
		camTransform = main.transform;
		camController = main.GetComponent<CamController>();
		squadCanvas = squadFrames;
		layoutCanvas = unitLayout;
		cardCanvas = unitCard;
		cameraSources = main.GetComponents<AudioSource>();
		unitTable = GetComponent<UnitTable>();
		unitManager = GetComponent<UnitManager>();
		soundManager = GetComponent<SoundManager>();
		objectPool = GetComponent<ObjectPool>();
		combatSlider = combatSliderRatio;

		Ground = ground.value;
		Unit = unit.value;
		Obstacle = obstacle.value;
		Squad = squad.value;		
		Water = water.value;
		
		ChargedInFlank = chargedInFlank.id;
		ChargedInRear = chargedInRear.id;
		Disordered = disordered.id;
		Exhausted = exhausted.id;
		FlanksProtected = flanksProtected.id;
		FlanksThreatened = flanksThreatened.id;
		FoodVariety = foodVariety.id;
		GeneralAround = generalAround.id;
		GeneralWound = generalWound.id;
		LosingAttack = losingAttack.id;
		LosingBattle = losingBattle.id;
		LowGround = lowGround.id;
		Marauding = marauding.id;
		NoEnemies = noEnemies.id;
		NoRetreatOption = noRetreatOption.id;
		OutnumberEnemy = outnumberEnemy.id;
		Outnumbered = outnumbered.id;
		Rain = rain.id;
		RoutingEnemies = routingEnemies.id;
		RoutingFriends = routingFriends.id;
		Starvation = starvation.id;
		TotallyExhausted = totallyExhausted.id;
		UnderFire = underFire.id;
		Unity = unity.id;
		UphillPosition = uphillPosition.id;
		VeryTired = veryTired.id;
		WinningBattle = winningBattle.id;
		WithoutAmmo = withoutAmmo.id;
	}
}