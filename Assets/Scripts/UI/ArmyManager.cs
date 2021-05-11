using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class ArmyManager : SingletonObject<ArmyManager>
{
    [Header("Main")]
    public TownController townController;
    public DescriptionWindow descriptionWindow;
    
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

    private Collider[] colliders = new Collider[1];

    public Party player => army.data;

    public void Init(Army target)
    {
        army = target;
        camController.SetTarget(army.worldTransform);
        enabled = true;
    }
    
    private void Start()
    {
        eventSystem = EventSystem.current;
        camera = Manager.mainCamera;
        camController = Manager.camController;
        border = Manager.border;
        enabled = false;
    }

    private void Update()
    {
        if (eventSystem.IsPointerOverGameObject() && !army.IsVisible())
            return;
        
        groundRay = camera.ScreenPointToRay(Input.mousePosition);
        groundCast = Physics.Raycast(groundRay, out groundHit, Manager.TerrainDistance, Manager.Ground);
        
        if (groundCast && !border.IsOutsideBorder(groundHit.point)) {
            if (Input.GetMouseButtonDown(1)) {
                if (Physics.Raycast(groundRay, out var hit, Manager.TerrainDistance, Manager.Building | Manager.Army)) {
                    var obj = hit.transform.gameObject;
                    var town = TownTable.Instance[obj];
                    if (town) {
                        army.SetDestination(town.entrance.position, town);
                    } else {
                        var enemy = ArmyTable.Instance[obj];
                        army.SetDestination(enemy.GetPosition(), enemy);
                    }
                } else {
                    army.SetDestination(groundHit.point);
                }

                camController.SetTarget(army.worldTransform);
            } else {
                OnHover();
            }
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