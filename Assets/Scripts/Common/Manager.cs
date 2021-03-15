﻿using UnityEngine;

 [RequireComponent(typeof(Manager))]
public class Manager : MonoBehaviour
{
	[Header("Camera")]
	public Camera main;
	public Camera minimap;
	public Camera ui;

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
	public static Camera mainCamera;
	public static Camera minimapCamera;
	public static Camera uiCamera;
	public static CamController controller;
	public static UnitTable unitTable;
	public static UnitManager unitManager;
	public static SoundManager soundManager;
	public static ObjectPooler objectPooler;
	
	private void Awake()
	{
		terrain = Terrain.activeTerrain;
		mainCamera = main;
		controller = main.GetComponent<CamController>();
		minimapCamera = minimap;
		uiCamera = ui;
		unitTable = GetComponent<UnitTable>();
		unitManager = GetComponent<UnitManager>();
		soundManager = GetComponent<SoundManager>();
		objectPooler = GetComponent<ObjectPooler>();

		Ground = ground.value;
		Unit = unit.value;
		Obstacle = obstacle.value;
		Squad = squad.value;		
		Water = water.value;
	}
}