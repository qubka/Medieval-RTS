﻿using System;
 using System.Collections.Generic;
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

	public static int Ground { get; private set; }
	public static int Army { get; private set; }
	public static int Unit { get; private set; }
	public static int Obstacle { get; private set; }
	public static int Building { get; private set; }
	public static int Squad { get; private set; }
	public static int Water { get; private set; }

	public static Global global { get; private set; }
	public static Terrain terrain { get; private set; }
	public static TerrainBorder border { get; private set; }
	public static Camera mainCamera { get; private set; }
	public static Camera minimapCamera { get; private set; }
	public static Transform cameraTransform { get; private set; }
	public static CameraController cameraController { get; private set; }
	public static AudioSource[] cameraSources { get; private set; }
	public static RectTransform holderCanvas { get; private set; }
	public static RectTransform layoutCanvas { get; private set; }
	public static RectTransform cardCanvas { get; private set; }
	public static GPUICrowdManager modelManager { get; private set; }
	public static TooltipPopup fixedPopup { get; private set; }
	public static TooltipPopup dynamicPopup { get; private set; }
	public static ChartPopup chartPopup { get; private set; }
	public static EventSystem eventSystem { get; private set; }
	
	public static MoraleAttribute ChargedInFlank { get; private set; }
	public static MoraleAttribute ChargedInRear { get; private set; }
	public static MoraleAttribute Disordered { get; private set; }
	public static MoraleAttribute Exhausted { get; private set; }
	public static MoraleAttribute FlanksProtected { get; private set; }
	public static MoraleAttribute FlanksThreatened { get; private set; }
	public static MoraleAttribute FoodVariety { get; private set; }
	public static MoraleAttribute GeneralAround { get; private set; }
	public static MoraleAttribute GeneralWound { get; private set; }
	public static MoraleAttribute LosingAttack { get; private set; }
	public static MoraleAttribute LosingBattle { get; private set; }
	public static MoraleAttribute LowGround { get; private set; }
	public static MoraleAttribute Marauding { get; private set; }
	public static MoraleAttribute NoEnemies { get; private set; }
	public static MoraleAttribute NoRetreatOption { get; private set; }
	public static MoraleAttribute OutnumberEnemy { get; private set; }
	public static MoraleAttribute Outnumbered { get; private set; }
	public static MoraleAttribute Rain { get; private set; }
	public static MoraleAttribute RoutingEnemies { get; private set; }
	public static MoraleAttribute RoutingFriends { get; private set; }
	public static MoraleAttribute Starvation { get; private set; }
	public static MoraleAttribute TotallyExhausted { get; private set; }
	public static MoraleAttribute UnderFire { get; private set; }
	public static MoraleAttribute Unity { get; private set; }
	public static MoraleAttribute UphillPosition { get; private set; }
	public static MoraleAttribute VeryTired { get; private set; }
	public static MoraleAttribute WinningBattle { get; private set; }
	public static MoraleAttribute WithoutAmmo { get; private set; }
	public static MoraleAttribute[] MoraleAttributes { get; private set; }
	
	public static float TerrainDistance { get; private set; }
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
		cameraTransform = main.transform;
		cameraController = main.GetComponent<CameraController>();
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
		
		MoraleAttributes = new[] {
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
	}

	public static bool IsPointerOnUI => eventSystem.IsPointerOverGameObject();

	private void OnGUI()
	{
		if (GUI.Button(new Rect(Screen.width-200,0,200,50), "Save"))
		{
			SaveLoadManager.SaveGame("test", DateTime.Now.ToString("dd_MM_yy_HH_m_s"));
		}
	}
}