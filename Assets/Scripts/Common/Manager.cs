﻿using System.Collections.Generic;
 using System.Linq;
 using GPUInstancer.CrowdAnimations;
 using Unity.Mathematics;
 using UnityEngine;
 
[RequireComponent(typeof(Manager))]
public class Manager : SingletonObject<Manager>
{
	#region Refs
	
	[Header("Refs")]
	public GPUICrowdManager crowdManager;
	public RectTransform holderFrames;
	public RectTransform unitLayout;
	public RectTransform unitCard;
	public Camera main;
	public Camera minimap;

	#endregion
	
	#region Layers

	[Header("Layers")]
	public LayerMask ground = -1;
	public LayerMask army = -1;
	public LayerMask unit = -1;
	public LayerMask obstacle = -1;
	public LayerMask building = -1;
	public LayerMask squad = -1;
	public LayerMask manager = -1;
	public LayerMask water = -1;
	
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	
	public static int Ground;
	public static int Army;
	public static int Unit;
	public static int Obstacle;
	//public static int Building;
	public static int Squad;
	//public static int Manager;
	public static int Water;
	
	#endregion
	
	#region Morale
	
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
	
	#endregion
	
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
	
	public static List<MoraleAttribute> moraleAttributes;

	#region Other
	
	public static float TerrainDistance;
	public static readonly int Selector = "SelectorPoint".GetHashCode();
	public static readonly int Pointer = "PointerMove".GetHashCode();
	public static readonly int Way = "Way".GetHashCode();
	public static readonly int GrayscaleAmount = Shader.PropertyToID("_GrayscaleAmount");
	public static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
	
	#endregion

	protected override void Awake()
	{
		base.Awake();
		
		terrain = Terrain.activeTerrain;
		border = terrain.GetComponent<TerrainBorder>();
		modelManager = crowdManager;
		mainCamera = main;
		minimapCamera = minimap;
		camTransform = main.transform;
		camController = main.GetComponent<CamController>();
		cameraSources = main.GetComponents<AudioSource>();
		holderCanvas = holderFrames;
		layoutCanvas = unitLayout;
		cardCanvas = unitCard;
		
		var size = terrain.terrainData.size;
		TerrainDistance = math.max(size.x, size.z) * 2f;

		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		
		Ground = ground.value;
		Army = army.value;
		Unit = unit.value;
		Obstacle = obstacle.value;
		Squad = squad.value;		
		Water = water.value;
		
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		
		ChargedInFlank = chargedInFlank;
		ChargedInRear = chargedInRear;
		Disordered = disordered;
		Exhausted = exhausted;
		FlanksProtected = flanksProtected;
		FlanksThreatened = flanksThreatened;
		FoodVariety = foodVariety;
		GeneralAround = generalAround;
		GeneralWound = generalWound;
		LosingAttack = losingAttack;
		LosingBattle = losingBattle;
		LowGround = lowGround;
		Marauding = marauding;
		NoEnemies = noEnemies;
		NoRetreatOption = noRetreatOption;
		OutnumberEnemy = outnumberEnemy;
		Outnumbered = outnumbered;
		Rain = rain;
		RoutingEnemies = routingEnemies;
		RoutingFriends = routingFriends;
		Starvation = starvation;
		TotallyExhausted = totallyExhausted;
		UnderFire = underFire;
		Unity = unity;
		UphillPosition = uphillPosition;
		VeryTired = veryTired;
		WinningBattle = winningBattle;
		WithoutAmmo = withoutAmmo;

		moraleAttributes = new List<MoraleAttribute>(32) {
			chargedInFlank,
			chargedInRear,
			disordered,
			exhausted,
			flanksProtected,
			flanksThreatened,
			foodVariety,
			generalAround,
			generalWound,
			losingAttack,
			losingBattle,
			lowGround,
			marauding,
			noEnemies,
			noRetreatOption,
			outnumberEnemy,
			outnumbered,
			rain,
			routingEnemies,
			routingFriends,
			starvation,
			totallyExhausted,
			underFire,
			unity,
			uphillPosition,
			veryTired,
			winningBattle,
			withoutAmmo
		};
	}
}