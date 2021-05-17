using System;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime;
using GPUInstancer;
using GPUInstancer.CrowdAnimations;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Army : MonoBehaviour, IGameObject
{
    [Header("Main Information")]
    public Party data;
    
    [Header("Children References")]
    //[Space(10f)]
    public GameObject armyIcon;
    private TextMeshProUGUI iconText;
    private GameObject armyBanner;
    //[Space(10f)]
    //public GameObject troopLayout;
    //public GameObject troopCard;
    [Space(10f)]
    [HideInInspector] public Transform worldTransform;
    [HideInInspector] public Transform iconTransform;

    [Header("Misc")]
    public float canvasHeight;
    public Vector3 barScale = new Vector3(1f, 1f, 1f);
    public Vector3 bannerPosition = new Vector3(0f, 2f, 0f);
    
    #region Local
    
#pragma warning disable 108,114    
    private Camera camera;
    private Collider collider;
#pragma warning restore 108,114
    private NavMeshAgent agent;
    private ArmyManager armyManager;
    private GPUICrowdManager modelManager;
    private List<GPUInstancerPrefab> instances;
    private bool visible = true;

    public IGameObject Target {
        get => data.followingObject;
        private set => data.followingObject = value;
    }

    public bool IsPlayer => data.leader.isPlayer;

    #endregion
    
    private void Awake()
    {
        // Set up the squad components
        collider = GetComponent<Collider>();
        agent = GetComponent<NavMeshAgent>();
        armyIcon = Instantiate(armyIcon);
        iconTransform = armyIcon.transform;
        worldTransform = transform;
    }

    private void Start()
    {
        // Get information from manager
        camera = Manager.mainCamera;
        //camController = Manager.camController;
        modelManager = Manager.modelManager;

        // Create bar if exist
        var house = data.leader.house;
        if (house) {
            var banner = Instantiate(house.banner.clearArmy, worldTransform);
            banner.AddComponent<BlockRotation>().worldTransform.localPosition = bannerPosition;
            armyBanner = banner;
        }

        // Attach scripts
        if (IsPlayer) {
            armyManager = ArmyManager.Instance;
            armyManager.SetPlayer(this);
        } else {
            var tree = gameObject.AddComponent<BehaviorTree>();
            tree.ExternalBehavior = data.leader.faction.behavior;
        }
        
        // Add a army to the tables
        ArmyTable.Instance.Add(gameObject, this);
        ObjectList.Instance.Add(this);

        // Parent a bar to the screen
        iconText = iconTransform.GetComponentInChildren<TextMeshProUGUI>();
        iconText.color = IsPlayer ? Color.green : (Color) data.leader.faction.color;
        iconText.text = data.troopCount.ToString();
        iconTransform.SetParent(Manager.holderCanvas, false);
        iconTransform.localScale = barScale;
        
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
        
        // TODO: Finish save for town waiting
        /*if (data.localTown) {
            Debug.Log("Entered city!");
            SetVisibility(false);
        }*/
    }

    public void Update()
    {
        if (Target != null) {
            if (!Target.IsVisible()) {
                Target = null;
            } else if (agent.IsArrived()) {
                switch (Target.GetUI()) {
                    case UI.Settlement:
                        TownInteraction(Target as Town);
                        worldTransform.position = Target.GetPosition();
                        break;
                    case UI.Army:
                        break;
                }
                Target = null;
            } else {
                // Update destination for player only
                if (IsPlayer && Target.GetUI() == UI.Army) {
                    agent.SetDestination(Target.GetPosition());
                }
            }
        }
        
        var pos = worldTransform.position;
        data.position = pos;
        data.rotation = worldTransform.rotation;

        if (visible) {
            // Calculate position for the ui bar
            pos.y += canvasHeight;
            pos = camera.WorldToScreenPoint(pos);
        
            // If the army is behind the camera, or too far away from the player, make sure to hide the bar completely
            if (pos.z < 0f) {
                armyIcon.SetActive(false);
            } else {
                iconTransform.position = pos;
                armyIcon.SetActive(true);
            }
        
            // TODO: Remove
            //barText.text = troopCount.ToString();
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

    public void SetVisibility(bool value)
    {
        if (visible == value)
            return;
        
        if (value) {
            foreach (var prefab in instances) {
                GPUInstancerAPI.AddPrefabInstance(modelManager, prefab);
                prefab.gameObject.SetActive(true);
            }
        } else {
            foreach (var prefab in instances) {
                GPUInstancerAPI.RemovePrefabInstance(modelManager, prefab);
                prefab.gameObject.SetActive(false);
            }
        }
        armyIcon.SetActive(value);
        armyBanner.SetActive(value);
        visible = value;
    }

    public void SetDestination(Vector3 position, IGameObject enemy = null)
    {
        if (data.localTown && ReferenceEquals(data.localTown, enemy))
            return;
        
        agent.SetDestination(position);
        Target = enemy;
        
        if (data.localTown) {
            var door = data.localTown.entrance;
            TownInteraction(null);
            worldTransform.SetPositionAndRotation(door.position, door.rotation);
        }
    } 
    
    public void TownInteraction(Town town)
    {
        if (town) {
            Debug.Log("Entered city!");
            town.data.parties.Add(data);
        } else {
            Debug.Log("Leave city!");
            data.localTown.data.parties.Remove(data);
        }
        SetVisibility(!town);
        if (IsPlayer) {
            var controller = armyManager.townController;
            controller.Toggle(town);
            controller.OnUpdate();
        }
        data.localTown = town;
    }

    #region Base

    public int GetID()
    {
        return data.leader.id;
    }
    
    public Vector3 GetPosition()
    {
        return worldTransform.position;
    }

    public Transform GetIcon()
    {
        return iconTransform;
    }

    public UI GetUI()
    {
        return UI.Army;
    }

    public bool IsVisible()
    {
        return visible;
    }

    #endregion
}
