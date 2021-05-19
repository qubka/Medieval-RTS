﻿﻿using System.Collections.Generic;
 using System.Linq;
 using GPUInstancer.CrowdAnimations;
 using Unity.Mathematics;
 using UnityEngine;
 using UnityEngine.EventSystems;

 public class Manager : SingletonObject<Manager>
{
	[Header("Refs")] 
	public bool isCheat;
	public Global globalInfo;
	public GPUICrowdManager crowdManager;
	[Space]
	public RectTransform holderFrames;
	public RectTransform layoutGroup;
	public RectTransform cardGroup;
	[Space]
	public Camera main;
	public Camera minimap;
	[Space]
	public TooltipPopup fix;
	public TooltipPopup dynamic;
	public ChartPopup chart;

	public static int Ground;
	public static int Army;
	public static int Unit;
	public static int Obstacle;
	public static int Building;
	public static int Squad;
	public static int Water;

	public static Global global;
	public static Terrain terrain;
	public static TerrainBorder border;
	public static Camera mainCamera;
	public static Camera minimapCamera;
	public static Transform camTransform;
	public static CamController camController;
	public static AudioSource[] cameraSources;
	public static RectTransform holderCanvas;
	public static RectTransform layoutCanvas;
	public static RectTransform cardCanvas;
	public static GPUICrowdManager modelManager;
	public static TooltipPopup fixedPopup;
	public static TooltipPopup dynamicPopup;
	public static ChartPopup chartPopup;
	public static EventSystem eventSystem;
	
	public static MoraleAttribute ChargedInFlank;
	public static MoraleAttribute ChargedInRear;
	public static MoraleAttribute Disordered;
	public static MoraleAttribute Exhausted;
	public static MoraleAttribute FlanksProtected;
	public static MoraleAttribute FlanksThreatened;
	public static MoraleAttribute FoodVariety;
	public static MoraleAttribute GeneralAround;
	public static MoraleAttribute GeneralWound;
	public static MoraleAttribute LosingAttack;
	public static MoraleAttribute LosingBattle;
	public static MoraleAttribute LowGround;
	public static MoraleAttribute Marauding;
	public static MoraleAttribute NoEnemies;
	public static MoraleAttribute NoRetreatOption;
	public static MoraleAttribute OutnumberEnemy;
	public static MoraleAttribute Outnumbered;
	public static MoraleAttribute Rain;
	public static MoraleAttribute RoutingEnemies;
	public static MoraleAttribute RoutingFriends;
	public static MoraleAttribute Starvation;
	public static MoraleAttribute TotallyExhausted;
	public static MoraleAttribute UnderFire;
	public static MoraleAttribute Unity;
	public static MoraleAttribute UphillPosition;
	public static MoraleAttribute VeryTired;
	public static MoraleAttribute WinningBattle;
	public static MoraleAttribute WithoutAmmo;
	
	public static List<MoraleAttribute> moraleAttributes;
	public static List<Faction> defaultFactions;
	public static List<Character> defaultCharacters;
	public static List<Settlement> defaultSettlements;
	public static List<Party> defaultParties;
	public static List<House> defaultHouses;
	public static List<Building> defaultBuildings;
	
	public static float TerrainDistance;
	public static readonly int Selector = "SelectorPoint".GetHashCode();
	public static readonly int Pointer = "PointerMove".GetHashCode();
	public static readonly int Way = "Way".GetHashCode();
	public static readonly int GrayscaleAmount = Shader.PropertyToID("_GrayscaleAmount");
	public static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

	protected override void Awake()
	{
		base.Awake();
		
		eventSystem = EventSystem.current;
		terrain = Terrain.activeTerrain;
		border = terrain.GetComponent<TerrainBorder>();
		var size = terrain.terrainData.size;
		TerrainDistance = math.max(size.x, size.z) * 2f;

		mainCamera = main;
		minimapCamera = minimap;
		camTransform = main.transform;
		camController = main.GetComponent<CamController>();
		cameraSources = main.GetComponents<AudioSource>();

		fixedPopup = fix;
		dynamicPopup = dynamic;
		chartPopup = chart;
		modelManager = crowdManager;
		holderCanvas = holderFrames;
		layoutCanvas = layoutGroup;
		cardCanvas = cardGroup;
		global = globalInfo;
		
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		
		Ground = global.ground.value;
		Army = global.army.value;
		Unit = global.unit.value;
		Obstacle = global.obstacle.value;
		Building = global.building.value;
		Squad = global.squad.value;		
		Water = global.water.value;
		
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		
		ChargedInFlank = global.chargedInFlank;
		ChargedInRear = global.chargedInRear;
		Disordered = global.disordered;
		Exhausted = global.exhausted;
		FlanksProtected = global.flanksProtected;
		FlanksThreatened = global.flanksThreatened;
		FoodVariety = global.foodVariety;
		GeneralAround = global.generalAround;
		GeneralWound = global.generalWound;
		LosingAttack = global.losingAttack;
		LosingBattle = global.losingBattle;
		LowGround = global.lowGround;
		Marauding = global.marauding;
		NoEnemies = global.noEnemies;
		NoRetreatOption = global.noRetreatOption;
		OutnumberEnemy = global.outnumberEnemy;
		Outnumbered = global.outnumbered;
		Rain = global.rain;
		RoutingEnemies = global.routingEnemies;
		RoutingFriends = global.routingFriends;
		Starvation = global.starvation;
		TotallyExhausted = global.totallyExhausted;
		UnderFire = global.underFire;
		Unity = global.unity;
		UphillPosition = global.uphillPosition;
		VeryTired = global.veryTired;
		WinningBattle = global.winningBattle;
		WithoutAmmo = global.withoutAmmo;

		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		
		moraleAttributes = new List<MoraleAttribute>(32) {
			global.chargedInFlank,
			global.chargedInRear,
			global.disordered,
			global.exhausted,
			global.flanksProtected,
			global.flanksThreatened,
			global.foodVariety,
			global.generalAround,
			global.generalWound,
			global.losingAttack,
			global.losingBattle,
			global.lowGround,
			global.marauding,
			global.noEnemies,
			global.noRetreatOption,
			global.outnumberEnemy,
			global.outnumbered,
			global.rain,
			global.routingEnemies,
			global.routingFriends,
			global.starvation,
			global.totallyExhausted,
			global.underFire,
			global.unity,
			global.uphillPosition,
			global.veryTired,
			global.winningBattle,
			global.withoutAmmo
		};
		defaultFactions = Resources.LoadAll<Faction>("Factions/").ToList();
		defaultCharacters = Resources.LoadAll<Character>("Characters/").ToList();
		defaultSettlements = Resources.LoadAll<Settlement>("Settlements/").ToList();
		defaultParties = Resources.LoadAll<Party>("Parties/").ToList();
		defaultHouses = Resources.LoadAll<House>("Houses/").ToList();
		defaultBuildings = Resources.LoadAll<Building>("Buildings/").ToList();
	}

	public static bool IsPointerOnUI => eventSystem.IsPointerOverGameObject();
}