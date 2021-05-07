﻿using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class ArmyManager : SingletonObject<ArmyManager>
{
#pragma warning disable 108,114
    private Camera camera;
#pragma warning restore 108,114
    private EventSystem eventSystem;
    private CamController camController;
    private TerrainBorder border;
    private Army army;
    private Army hover;
    private float nextHoverTime;
    private Ray groundRay;
    private RaycastHit groundHit;
    private bool groundCast;
    private bool paused;
    private bool moveToSettlement;
    
    private Collider[] colliders = new Collider[1];

    protected override void Awake()
    {
        base.Awake();
        
        army = GetComponent<Army>();
    }

    private void Start()
    {
        // Get information from manager
        //eventSystem = EventSystem.current;
        camera = Manager.mainCamera;
        camController = Manager.camController;
        border = Manager.border;
        
        // Set the camera target to follow
        camController.SetTarget(army.worldTransform);
    }
    
    private void Update()
    {
        groundRay = camera.ScreenPointToRay(Input.mousePosition);
        groundCast = Physics.Raycast(groundRay, out groundHit, Manager.TerrainDistance, Manager.Ground);
        //pointerUI = eventSystem.IsPointerOverGameObject();

        if (groundCast && !border.IsOutsideBorder(groundHit.point)) {
            if (Input.GetMouseButtonDown(1)) {
                if (Physics.Raycast(groundRay, out var hit, Manager.TerrainDistance, Manager.Building)) {
                    army.agent.SetDestination(TownTable.Instance[hit.transform.gameObject].entrance.position);
                    moveToSettlement = true;
                } else {
                    army.agent.SetDestination(groundHit.point);
                    moveToSettlement = false;
                }
                camController.SetTarget(army.worldTransform);
            } else {
                OnHover();
            }
        }

        //TODO: REWORK
        if (moveToSettlement && army.agent.IsArrived()) {
            Debug.Log("Arrived!");
            moveToSettlement = false;
        }
    }

    private void OnHover()
    {
        var currentTime = Time.unscaledTime;
        if (currentTime > nextHoverTime) {
            if (!HoverOnTarget() && hover) {
                hover = null;
            }
            nextHoverTime = currentTime + 0.1f;
        }
    }

    private bool HoverOnTarget()
    {
        if (Physics.OverlapSphereNonAlloc(groundHit.point, 2f, colliders, Manager.Army) != 0) {
            hover = ArmyTable.Instance[colliders[0].gameObject];
            //armyInfo.OnUpdate();
            return true;
        } 
        
        return false;
    }
}