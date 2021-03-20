﻿using GPUInstancer.CrowdAnimations;
 using UnityEngine;
 
[RequireComponent(typeof(Manager))]
public class Manager : MonoBehaviour
{
	[Header("Refs")]
	public GPUICrowdManager crowdManager;
	public RectTransform squadFrames;
	public RectTransform unitLayout;
	public RectTransform unitGrid;
	public Camera main;
	public Camera minimap;

	[Header("Layers")] 
	public LayerMask ui = -1;
	public LayerMask ground = -1;
	public LayerMask unit = -1;
	public LayerMask obstacle = -1;
	public LayerMask building = -1;
	public LayerMask squad = -1;
	public LayerMask manager = -1;
	public LayerMask water = -1;

	public static int UI;
	public static int Ground;
	public static int Unit;
	public static int Obstacle;
	//public static int Building;
	public static int Squad;
	//public static int Manager;
	public static int Water;
	
	public static Terrain terrain;
	public static Camera mainCamera;
	public static Camera minimapCamera;
	public static Transform cameraTransform;
	public static RectTransform squadCanvas;
	public static RectTransform layoutCanvas;
	public static RectTransform gridCanvas;
	public static CamController controller;
	public static GPUICrowdManager modelManager;
	public static UnitTable unitTable;
	public static UnitManager unitManager;
	public static SoundManager soundManager;
	public static ObjectPooler objectPooler;
	
	private void Awake()
	{
		terrain = Terrain.activeTerrain;
		modelManager = crowdManager;
		mainCamera = main;
		minimapCamera = minimap;
		cameraTransform = main.transform;
		squadCanvas = squadFrames;
		layoutCanvas = unitLayout;
		gridCanvas = unitGrid;
		controller = main.GetComponent<CamController>();
		unitTable = GetComponent<UnitTable>();
		unitManager = GetComponent<UnitManager>();
		soundManager = GetComponent<SoundManager>();
		objectPooler = GetComponent<ObjectPooler>();

		UI = ui.value;
		Ground = ground.value;
		Unit = unit.value;
		Obstacle = obstacle.value;
		Squad = squad.value;		
		Water = water.value;
	}
}