using System;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime;
using GPUInstancer;
using GPUInstancer.CrowdAnimations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Army : MonoBehaviour, ISortable
{
    [Header("Main Information")]
    public Party data;
    
    [Header("Children References")]
    public GameObject armyBar;
    private Text barText;
    [Space(10f)]
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Transform worldTransform;
    [HideInInspector] public Transform barTransform;

    [Header("Misc")]
    public float canvasHeight;
    public Vector3 barScale = new Vector3(1f, 1f, 1f);
    public Vector3 bannerPosition = new Vector3(0f, 2f, 0f);
    
    #region Local
    
    private Camera cam;
    //private CamController camController;
    private GPUICrowdManager modelManager;
    private SortList sortList;
    private ArmyTable armyTable;
    private RectTransform holderCanvas;
    private List<GPUInstancerPrefab> instances;

    public int troopCount => data.troops.Sum(t => t.size);
    
    #endregion
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        armyBar = Instantiate(armyBar);
        barTransform = armyBar.transform;
        worldTransform = transform;

        var banner = data.leader.banner;
        if (banner) {
            Instantiate(banner.clearArmy, worldTransform).AddComponent<BlockRotation>().worldTransform.localPosition = bannerPosition;
        }

        if (data.leader.isPlayer) {
            gameObject.AddComponent<ArmyManager>();
        } else {
            var tree = gameObject.AddComponent<BehaviorTree>();
            tree.ExternalBehavior = data.behavior;
        }
    }

    private void Start()
    {
        // Get information from manager
        cam = Manager.mainCamera;
        modelManager = Manager.modelManager;
        holderCanvas = Manager.holderCanvas;
        sortList = Manager.sortList;
        armyTable = Manager.armyTable;
        
        // Add a army to the tables
        armyTable.Add(gameObject, this);
        sortList.Add(this);

        // Parent a bar to the screen
        barText = barTransform.GetComponentInChildren<Text>();
        barText.color = data.leader.isPlayer ? Color.green : data.leader.faction.color;
        barText.text = troopCount.ToString();
        barTransform.SetParent(holderCanvas, false);
        barTransform.localScale = barScale;
        
        // Disabling the Crowd Manager here to change prototype settings
        // Enabling it after this will make it re-initialize with the new settings for the prototypes
        modelManager.enabled = false;
        
        // Setup the first prototype in the manager
        instances = new List<GPUInstancerPrefab>(2);
        
        // Initialize the crowds
        var faction = data.leader.faction.models[data.skin];
        if (faction.primary) instances.Add(CreateCrowd(faction.primary));
        if (faction.secondary) instances.Add(CreateCrowd(faction.secondary));
        
        // Register the instantiated GOs to the Crowd Manager
        GPUInstancerAPI.RegisterPrefabInstanceList(modelManager, instances);
        GPUInstancerAPI.InitializeGPUInstancer(modelManager);
        
        // Enabling the Crowd Manager back; this will re-initialize it with the new settings for the prototypes
        modelManager.enabled = true;
    }

    public void Update()
    {
        // Calculate position for the ui bar
        var center = worldTransform.position;
        center.y += canvasHeight;
        center = cam.WorldToScreenPoint(center);
        
        // If the army is behind the camera, or too far away from the player, make sure to hide the health bar completely
        if (center.z < 0f) {
            armyBar.SetActive(false);
        } else {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(holderCanvas, center, null, out var canvasPos);
            barTransform.localPosition = canvasPos;
            armyBar.SetActive(true);
        }
    }
    
    private GPUICrowdPrefab CreateCrowd(GameObject prefab)
    {
        var obj = Instantiate(prefab, worldTransform);
        var crowd = obj.GetComponent<GPUICrowdPrefab>();

        var animator = obj.GetComponent<AnimatedCrowd>();
        animator.agent = agent;
        animator.crowd = crowd;
        return crowd;
    }
    
    #region Sorting

    public Vector3 GetPosition()
    {
        return worldTransform.position;
    }

    public Transform GetTransform()
    {
        return barTransform;
    }

    #endregion

    private void OnTriggerStay(Collision collision)
    {
        var other = collision.collider.gameObject;
        
        var army = other.GetComponent<Army>();
        if (army) {
            Debug.Log(army.name);
        }
    }
}
