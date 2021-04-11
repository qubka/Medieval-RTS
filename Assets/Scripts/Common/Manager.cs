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
	public SquadDescription squadDescriptionBoard;

	[Header("Layers")]
	public LayerMask ground = -1;
	public LayerMask unit = -1;
	public LayerMask obstacle = -1;
	public LayerMask building = -1;
	public LayerMask squad = -1;
	public LayerMask manager = -1;
	public LayerMask water = -1;
	
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
	public static SquadDescription squadDesc;
	public static AudioSource[] cameraSources;
	
	public static readonly int Selector = "SelectorPoint".GetHashCode();
	public static readonly int Pointer = "PointerMove".GetHashCode();
	public static readonly int Way = "Way".GetHashCode();

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
		squadDesc = squadDescriptionBoard;

		Ground = ground.value;
		Unit = unit.value;
		Obstacle = obstacle.value;
		Squad = squad.value;		
		Water = water.value;
	}
}