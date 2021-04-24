using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ArmyManager : MonoBehaviour
{
    private Collider[] colliders = new Collider[1];
    private EventSystem eventSystem;
    private TerrainBorder border;
    private Camera cam;
    private ArmyTable armyTable;
    private Army army;
    private Army hover;
    private float nextHoverTime;
    private Ray groundRay;
    private RaycastHit groundHit;
    private bool groundCast;
    private bool paused;
    //private bool pointerUI;

    private void Awake()
    {
        army = GetComponent<Army>();
    }

    private void Start()
    {
        // Get information from manager
        eventSystem = EventSystem.current;
        cam = Manager.mainCamera;
        border = Manager.border;
        armyTable = Manager.armyTable;
    }
    
    private void Update()
    {
        groundRay = cam.ScreenPointToRay(Input.mousePosition);
        groundCast = Physics.Raycast(groundRay, out groundHit, Manager.TerrainDistance, Manager.Ground);
        //pointerUI = eventSystem.IsPointerOverGameObject();

        if (groundCast && !border.IsOutsideBorder(groundHit.point)) {
            if (Input.GetMouseButtonDown(1)) {
                army.agent.SetDestination(groundHit.point);
            } else {
                OnHover();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
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
            hover = armyTable[colliders[0].gameObject];
            //armyInfo.OnUpdate();
            return true;
        } 
        
        return false;
    }
}